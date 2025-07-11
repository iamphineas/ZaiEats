using System.ComponentModel.DataAnnotations;

namespace ZaiEats.ViewModels
{
    public class PodcastEpisodeViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Url]
        public string FullEpisodeUrl { get; set; }

        public string Tags { get; set; }
    }
}

