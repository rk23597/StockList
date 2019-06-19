using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StockList.Helper;
using StockList.Models;

namespace StockList.Controllers
{
    [Route("api/[controller]")]
    public class FileController : Controller
    {


        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;

        public FileController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        // POST /api/file/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)

                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.FileContainer == string.Empty)

                    return BadRequest("Please provide a name for your file container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsCSV(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }
                if (isUploaded)
                {
                    return Ok("Successfully uploaded");
                }
                else

                    return BadRequest("Look like the file couldnt upload to the storage");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}