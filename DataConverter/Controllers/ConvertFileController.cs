using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;

namespace DataConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertFileController : ControllerBase
    {
        public IMailer Mailer = new Mailer();

        /// <summary>
        /// Converts the attached file to the specified data format.
        /// </summary>
        /// <param name="outputFormat">The desired format the attached file should be converted to.</param>
        /// <param name="file">The file to convert.</param>
        /// <param name="email">(Optional) Send the converted file to given e-mail address.</param>
        /// <returns>Returns the converted file.</returns>
        /// <response code="200">On success. (Returned file or sent e-mail)</response>
        /// <response code="204">If the attached file is null or empty.</response>
        /// <response code="400">If the input or output content type is not supported.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult FromFile([Required] string outputFormat, [Required] IFormFile file, string? email)
        {
            try
            {
                var outputContentType = $"application/{outputFormat}";
                
                if (file.Length > 0)
                {
                    using var streamReader = new StreamReader(file.OpenReadStream());
                    var input = streamReader.ReadToEnd();
                    var output = DataFormatConverter.Convert(input, file.ContentType, outputContentType);

                    if (email != null)
                    {
                        Mailer.EmailFileTo(email, file.FileName, output, outputContentType);
                        return Ok();
                    }
                    else
                    {
                        return File(Encoding.UTF8.GetBytes(output), outputContentType, Path.GetFileNameWithoutExtension(file.FileName));
                    }
                }
                else
                {
                    return NoContent();
                }
            }
            catch (InvalidContentTypeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Converts file from URL to the specified data format.
        /// </summary>
        /// <param name="inputFormat">The format of the file at URL.</param>
        /// <param name="outputFormat">The desired format the file from URL should be converted to.</param>
        /// <param name="fileUrl">The URL of the file to convert.</param>
        /// <param name="email">(Optional) Send the converted file to given e-mail address.</param>
        /// <returns>Returns the converted file.</returns>
        /// <response code="200">On success. (Returned file or sent e-mail)</response>
        /// <response code="400">If the input or output content type is not supported.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FromUrlAsync([Required] string inputFormat, [Required] string outputFormat, [Required] string fileUrl, string? email)
        {
            try
            {
                var inputContentType = $"application/{inputFormat}";
                var outputContentType = $"application/{outputFormat}";

                using var client = new HttpClient();
                var fileStream = await client.GetStreamAsync(fileUrl);
                using var streamReader = new StreamReader(fileStream);
                var input = streamReader.ReadToEnd();
                var output = DataFormatConverter.Convert(input, inputContentType, outputContentType);

                if(email != null)
                {
                    Mailer.EmailFileTo(email, Path.GetFileNameWithoutExtension(fileUrl), output, outputContentType);
                    return Ok();
                }
                else
                {
                    return File(Encoding.UTF8.GetBytes(output), outputContentType, Path.GetFileNameWithoutExtension(fileUrl));
                }
            }
            catch (InvalidContentTypeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}