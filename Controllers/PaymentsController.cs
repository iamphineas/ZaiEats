using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayFast;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Web;
using ZaiEats.Data;
using ZaiEats.Services;
using Microsoft.AspNetCore.Identity;
using ZaiEats.Models;

namespace ZaiEats.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PayFastSettings payFastSettings;
        private readonly ApplicationDbContext db;
        private readonly Cart _cart;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(IConfiguration configuration, ApplicationDbContext context, Cart cart, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _cart = cart;
            _userManager = userManager;
            this.payFastSettings = new PayFastSettings
            {
                MerchantId = configuration["MerchantId"],
                MerchantKey = configuration["MerchantKey"],
                PassPhrase = configuration["PassPhrase"],
                ProcessUrl = configuration["ProcessUrl"],
                ValidateUrl = configuration["ValidateUrl"],
                ReturnUrl = configuration["ReturnUrl"],
                CancelUrl = configuration["CancelUrl"],
                NotifyUrl = configuration["NotifyUrl"]
            };
        }

        /// <summary>
        /// Processes the payment for the winning bid of a given lead.
        /// </summary>
        /// <param name="leadId">The identifier for the lead whose winning bid is to be paid for.</param>
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            //var lead = db.Leads.Find(id);
            //if (id == null)
            //{
            //    return NotFound("Lead ID was not provided.");
            //}

            // get the current user object
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();  // not signed in

            // now you can read:
            var email = user.Email;

            var cartItems = _cart.GetItems();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            // 1) compute total
            var total = cartItems.Sum(ci => ci.LineTotal);
            string formattedAmount = total.ToString("F2", CultureInfo.InvariantCulture);

            var orderid= "ZAE" + cartItems.Select(CI => CI.MenuItemId).FirstOrDefault() + DateTime.Now.ToString("yyyyMMddHHmmss");

            // 2) build a single "item_name" and a multi-line "item_description"
            var nameList = string.Join(", ", cartItems.Select(ci => ci.Name));
            var descLines = cartItems
                .Select(ci => $"{ci.Name} x{ci.Quantity} = R{ci.LineTotal:F2}");
            var description = string.Join(" | ", descLines);


            // 3) generate a unique payment reference
            var mPaymentId = Guid.NewGuid().ToString();

            var paymentRequest = new PayFastRequest(this.payFastSettings.PassPhrase)
            {
                merchant_id = this.payFastSettings.MerchantId,
                merchant_key = this.payFastSettings.MerchantKey,
                return_url = this.payFastSettings.ReturnUrl,
                cancel_url = this.payFastSettings.CancelUrl,
                notify_url = this.payFastSettings.NotifyUrl,
                email_address = "sbtu01@payfast.co.za",
                m_payment_id = "8d00bf49-e979-4004-228c-08d452b86380",
                amount = (double)total,
                item_name = $"Payment for Lead #{orderid}",
                item_description = $"Winning Bid Payment for Lead: {description}",
                email_confirmation = true,
                confirmation_address = "sbtu01@payfast.co.za"
            };

            // Construct the base PayFast URL
            var processUrl = "https://sandbox.payfast.co.za/eng/process?";

            // Add the PayFast details directly to the URL
            processUrl += $"merchant_id=10004241&";
            processUrl += $"merchant_key=132ncgdwrh2by&";
            processUrl += $"return_url={HttpUtility.UrlEncode("https://localhost:44365/Payments/Return")}&";
            processUrl += $"cancel_url={HttpUtility.UrlEncode("https://payfast-demo-mvc.azurewebsites.net/home/cancel")}&";
            processUrl += $"notify_url={HttpUtility.UrlEncode("https://payfast-demo-mvc.azurewebsites.net/home/notify")}&";

            processUrl += $"email_address={HttpUtility.UrlEncode("sbtu01@payfast.co.za")}&";
            processUrl += $"m_payment_id={HttpUtility.UrlEncode("8d00bf49-e979-4004-228c-08d452b86380")}&";
            processUrl += $"amount={HttpUtility.UrlEncode(formattedAmount)}&";
            processUrl += $"item_name={HttpUtility.UrlEncode("Order " + orderid)}&";
            processUrl += $"item_description={HttpUtility.UrlEncode("description " + description)}&";
            processUrl += $"email_confirmation=1&";
            processUrl += $"confirmation_address={HttpUtility.UrlEncode("sbtu01@payfast.co.za")}&";

            //var processUrl = this.payFastSettings.ProcessUrl + "?";
            //processUrl += "merchant_id=" + HttpUtility.UrlEncode(this.payFastSettings.MerchantId) + "&";
            //processUrl += "merchant_key=" + HttpUtility.UrlEncode(this.payFastSettings.MerchantKey) + "&";
            //processUrl += "return_url=" + HttpUtility.UrlEncode(this.payFastSettings.ReturnUrl) + "&";
            //processUrl += "cancel_url=" + HttpUtility.UrlEncode(this.payFastSettings.CancelUrl) + "&";
            //processUrl += "notify_url=" + HttpUtility.UrlEncode(this.payFastSettings.NotifyUrl) + "&";
            //processUrl += "email_address=" + HttpUtility.UrlEncode(paymentRequest.email_address) + "&";
            //processUrl += "m_payment_id=" + HttpUtility.UrlEncode(paymentRequest.m_payment_id) + "&";
            //processUrl += "amount=" + HttpUtility.UrlEncode(paymentRequest.amount.ToString("F2")) + "&";
            //processUrl += "item_name=" + HttpUtility.UrlEncode(paymentRequest.item_name) + "&";
            //processUrl += "item_description=" + HttpUtility.UrlEncode(paymentRequest.item_description) + "&";
            //processUrl += "email_confirmation=1&";
            //processUrl += "confirmation_address=" + HttpUtility.UrlEncode(paymentRequest.confirmation_address);

            return Redirect(processUrl);
        }

        //method to send email 
        public void SendEmail(string orderid, string namelist, IEnumerable<string> desclines, string description, string formatedammount, string email)
        {
            //var user = _context.Users.Find(request.UserId);
            var emaill = email;
            string message =
                $@"
Hi there,

Thank you for shopping with ZaiEats! Here are the details of your order:

Order Number: {orderid}

Items:
{namelist}



Order Summary:
{description}

Total Amount Charged: R{formatedammount}

We’ll send you another email as soon as your order is on its way.

If you have any questions, just reply to this message.

Kind regards,
The ZaiEats Team
";

            // Sendemail
            var senderEmail = new MailAddress("skyglobalhigh@gmail.com", "ZAE Eats");
            var recieverMail = new MailAddress(email, "Client");
            var password = "gfas jrec lhtp gikc";
            var sub = $"Your ZaiEats Order Confirmation #{orderid}";
            var body = message;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                // EnableSsl = false,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, recieverMail)
            {
                Subject = sub,
                Body = body
            })
            {
                smtp.Send(mess);
            }
        }



        /// <summary>
        /// Return URL after payment processing.
        /// </summary>
        public async Task<IActionResult> Return()
        {

           


            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();  // not signed in

            // now you can read:
            var email = user.Email;

            var cartItems = _cart.GetItems();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            // 1) compute total
            var total = cartItems.Sum(ci => ci.LineTotal);
            string formattedAmount = total.ToString("F2", CultureInfo.InvariantCulture);

            var orderid = "ZAE" + cartItems.Select(CI => CI.MenuItemId).FirstOrDefault() + DateTime.Now.ToString("yyyyMMddHHmmss");

            // 2) build a single "item_name" and a multi-line "item_description"
            var nameList = string.Join(", ", cartItems.Select(ci => ci.Name));
            var descLines = cartItems
                .Select(ci => $"{ci.Name} x{ci.Quantity} = R{ci.LineTotal:F2}");
            var description = string.Join(" | ", descLines);


            // 3) generate a unique payment reference
            var mPaymentId = Guid.NewGuid().ToString();


            // 4) Send the confirmation email
            SendEmail(orderid, nameList, descLines, description, formattedAmount, email);

            // 5) Clear the cart if you like
            _cart.Clear();

            // You can process query strings from PayFast here to verify payment status.
            return View();
        }
    }
}
