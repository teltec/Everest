using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Backup.Models
{
	public enum CloudStorageAccountType
	{
		AMAZON_S3 = 1,
		FILESYSTEM = 2,
	}

    public interface ICloudStorageAccount
    {
        Guid Id { get; set; }

		CloudStorageAccountType Type { get; set; }

        String DisplayName { get; set; }

		//IList<BackupPlan> BackupPlans { get; set; }
    }
}
