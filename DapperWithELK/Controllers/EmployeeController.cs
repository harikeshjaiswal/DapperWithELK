using DapperWithELK.Model;
using DapperWithELK.Repo;
using DapperWithELK.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperWithELK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepo repo;
        private readonly IElasticsearchService<tbl_employee> elasticsearch;

        public EmployeeController(IEmployeeRepo repo,IElasticsearchService<tbl_employee> _elasticsearch)
        {
            this.repo = repo;
            elasticsearch = _elasticsearch;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
           // var _list = this.repo.GetAll();

           var _list = await elasticsearch.GetAllDocuments();

            if (_list != null)
            {
                return Ok(_list);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] tbl_employee employee)
        {
            var insertResult = this.repo.Insert(employee);

            if (insertResult != null)
            {
                // If the insertion into the database was successful, push to Elasticsearch
                var elasticResult = await elasticsearch.CreateDocumentAsync(employee);

                if (elasticResult == "Document created successfully")
                {
                    // Both database insertion and Elasticsearch document creation were successful
                    return Ok(insertResult);
                }
                else
                {
                    // Handle Elasticsearch document creation failure
                    // You might want to consider rolling back the database insertion here
                    // or implementing some compensation logic
                    return BadRequest("Failed to create Elasticsearch document");
                }
            }
            else
            {
                // Handle database insertion failure
                return BadRequest("Failed to insert into the database");
            }
        }


        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] tbl_employee employee)
        {
            var updateResult = this.repo.Update(employee);

            if (updateResult != null)
            {
                // If the update in the database was successful, update the corresponding document in Elasticsearch
                var elasticResult = await elasticsearch.UpdateDocumentAsync(employee);

                if (elasticResult == "Document updated successfully")
                {
                    // Both database update and Elasticsearch document update were successful
                    return Ok(updateResult);
                }
                else
                {
                    // Handle Elasticsearch document update failure
                    // You might want to consider rolling back the database update here
                    // or implementing some compensation logic
                    return BadRequest("Failed to update Elasticsearch document");
                }
            }
            else
            {
                // Handle database update failure
                return BadRequest("Failed to update the database");
            }
        }


        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(int id)
        {
            var resultFromDatabase = this.repo.GetBaseddOnCode(id);

            if (resultFromDatabase != null)
            {
                // If data exists in the database based on the provided ID, proceed with deletion
                var deleteResultFromDatabase = this.repo.Delete(resultFromDatabase);

                if (deleteResultFromDatabase != null)
                {
                    // If the database deletion was successful, delete the corresponding document in Elasticsearch
                    var elasticDeleteResult = await elasticsearch.DeleteDocumentAsync(id);

                    if (elasticDeleteResult == "Document deleted successfully")
                    {
                        // Both database deletion and Elasticsearch document deletion were successful
                        return Ok(deleteResultFromDatabase);
                    }
                    else
                    {
                        // Handle Elasticsearch document deletion failure
                        // You might want to consider rolling back the database deletion here
                        // or implementing some compensation logic
                        return BadRequest("Failed to delete Elasticsearch document");
                    }
                }
                else
                {
                    // Handle database deletion failure
                    return BadRequest("Failed to delete from the database");
                }
            }
            else
            {
                // Handle case where data with the provided ID does not exist in the database
                return NotFound("No data found for the provided ID");
            }
        }

    }
}
