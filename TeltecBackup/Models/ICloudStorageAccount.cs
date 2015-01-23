using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Backup.Models
{
    public interface ICloudStorageAccount
    {
        Guid Id { get; set; }

        String DisplayName { get; set; }
    }
}
