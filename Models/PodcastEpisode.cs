using System.ComponentModel.DataAnnotations;

namespace ZaiEats.Models
{
    public class PodcastEpisode
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string TeaserVideoPath { get; set; } // stores the relative path to uploaded video

        [Required]
        [Url]
        public string FullEpisodeUrl { get; set; } // YouTube link

        public string Tags { get; set; } // comma-separated tags

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    }
}

