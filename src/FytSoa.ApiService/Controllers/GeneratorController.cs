using FytSoa.Common.Result;
using FytSoa.Generator;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.ApiService.Controllers
{
    /// <summary>
    /// 代码生成
    /// </summary>
    public class GeneratorController : ApiController
    {
        private readonly IGeneratorService _generatorService;
        public GeneratorController(IGeneratorService generatorService)
        {
            _generatorService = generatorService;
        }
        
        [HttpGet("table")]
        public PageResult<DbTableInfo> GetTable()
        {
            return new PageResult<DbTableInfo>()
            {
                Items = _generatorService.InitTable()
            };
        }
        
        
        [HttpGet("column")]
        public PageResult<DbColumnInfo> GetTableColumn(string tableName)
        {
            return new PageResult<DbColumnInfo>()
            {
                Items =_generatorService.GetColumn(tableName)
            };
        }
        
        [HttpPost]
        public string Post([FromBody]GeneratorTableDto param)
        {
            return  _generatorService.CreateCode(param);
        }
    }
}