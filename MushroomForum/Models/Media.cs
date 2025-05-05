using MushroomForum.Models;
using System.ComponentModel.DataAnnotations;

public class Media
{
    public int MediaId { get; set; }
    [MaxLength(512)]
    public string Url { get; set; }
    [MaxLength(50)]
    public string Type { get; set; } //film, zdjęcie
    public int PostId { get; set; }
    public Post Post { get; set; }
}