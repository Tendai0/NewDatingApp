using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class AppUser
{
public int id { get; set; }
public string UserName { get; set; }
public byte[] PassWordHarsh { get; set; }
public byte[] PassWordSalt { get; set; }
}
