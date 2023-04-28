using System.Text;
using FytSoa.Common.Utils;
using FytSoa.Generator.Utils;
using FytSoa.Sugar;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Hosting;
using SqlSugar;

namespace FytSoa.Generator;

public class GeneratorService : SugarRepository<GeneratorService>, IGeneratorService
{
    /// <summary>
    /// 生成代码
    /// </summary>
    /// <returns></returns>
    public string CreateCode(GeneratorTableDto createModel)
    {
        var path = AppUtils.AppRoot + "/upload/";
        FileUtils.CreateSuffic(path);

        //读取模板——实体
        var modelTemp = FileUtils.ReadFile(path + @"Template/Model.txt");
        //读取模板——Dto
        var dtoTemp = FileUtils.ReadFile(path + @"Template/Dto.txt");
        //读取模板——服务实现
        var serviceTemp = FileUtils.ReadFile(path + @"Template/Service.txt");
        //读取模板Tree——服务实现
        var treeServiceTemp = FileUtils.ReadFile(path + @"Template/TreeService.txt");

        //读取模板Js——前端
        var jsTemp = FileUtils.ReadFile(path + @"Template/Vue/Js.txt");
        //读取模板List——前端
        var listTemp = FileUtils.ReadFile(path + @"Template/Vue/List.txt");
        //读取模板Modify——前端
        var modifyTemp = FileUtils.ReadFile(path + @"Template/Vue/Modify.txt");


        //判断是否存在树形结构的实体，如果存在，则使用Tree模板
        bool isParent = false, isParentList = false, isLayer = false;
        //构建属性
        string attrStr = "", dtoAttrStr = "", tableColumn = "";
        foreach (var item in createModel.TableColumnInfo)
        {
            if (EntityConstant.IgnoreDtoColumn.Contains(item.DbColumnName))
            {
                continue;
            }

            switch (item.DbColumnName)
            {
                case "ParentId":
                    isParent = true;
                    break;
                case "ParentIdList":
                    isParentList = true;
                    break;
                case "Layer":
                    isLayer = true;
                    break;
            }

            if (!item.IsPrimarykey)
            {
                attrStr += "    /// <summary>\r\n";
                attrStr += "    /// " + item.ColumnDescription + "\r\n";
                attrStr += "    /// </summary>\r\n";
                if (!item.IsNullable && item.DataType.ConvertModelType() == "string")
                {
                    attrStr += "    [Required]\r\n";
                    attrStr += "    [StringLength(" + item.Length + ")]\r\n";
                }

                if (!item.IsNullable && item.DataType.ConvertModelType() != "string")
                {
                    attrStr += "    [Required]\r\n";
                }

                attrStr += "    public " + item.DataType.ConvertModelType(item.IsNullable) + " " + item.DbColumnName +
                           " { get; set; }" + item.DataType.ModelDefaultValue(item.DefaultValue, item.IsNullable) +
                           "\r\n\r\n";
            }

            dtoAttrStr += "    /// <summary>\r\n";
            dtoAttrStr += "    /// " + item.ColumnDescription + "\r\n";
            dtoAttrStr += "    /// </summary>\r\n";
            if (!item.IsNullable && item.DataType.ConvertModelType() == "string")
            {
                dtoAttrStr += "    [Required]\r\n";
                dtoAttrStr += "    [StringLength(" + item.Length + ")]\r\n";
            }

            if (!item.IsNullable && item.DataType.ConvertModelType() != "string" && !item.IsPrimarykey)
            {
                dtoAttrStr += "    [Required]\r\n";
            }

            dtoAttrStr += "    public " + item.DataType.ConvertModelType(item.IsNullable) + " " + item.DbColumnName +
                          " { get; set; }" + item.DataType.ModelDefaultValue(item.DefaultValue, item.IsNullable) +
                          "\r\n\r\n";
        }

        var table = Context.DbMaintenance.GetTableInfoList().First(m => createModel.TableNames.Contains(m.Name));
        var modelName = table.Name.TableName();
        tableColumn = modelTemp
            .Replace("{NameSpace}", createModel.Namespace)
            .Replace("{TableNameDescribe}", table.Description)
            .Replace("{DataTable}", table.Name)
            .Replace("{TableName}", modelName)
            .Replace("{AttributeList}", attrStr);
        //写入文件-Model
        FileUtils.WriteFile(path + "Generator/Model/" + createModel.Namespace + "/", modelName + ".cs", tableColumn);

        var serverPath = path + "Generator/" + createModel.Namespace + "/" + modelName + "Service/";

        //Dto
        string dtoString = dtoTemp.Replace("{NameSpace}", createModel.Namespace)
            .Replace("{TableNameDescribe}", table.Description.Replace("\r\n", "/"))
            .Replace("{TableName}", modelName)
            .Replace("{AttributeList}", dtoAttrStr);
        FileUtils.WriteFile(serverPath + "/Dto/", modelName + "Dto.cs", dtoString);

        if (!isParent && !isParentList && !isLayer)
        {
            //接口实现
            string serviceString = serviceTemp.Replace("{NameSpace}", createModel.Namespace)
                .Replace("{TableNameDescribe}", table.Description.Replace("\r\n", "/"))
                .Replace("{Version}", createModel.ApiVersion)
                .Replace("{TableName}", modelName);
            FileUtils.WriteFile(serverPath, modelName + "Service.cs", serviceString);
        }
        else
        {
            //接口实现
            string treeServiceString = treeServiceTemp.Replace("{NameSpace}", createModel.Namespace)
                .Replace("{TableNameDescribe}", table.Description.Replace("\r\n", "/"))
                .Replace("{Version}", createModel.ApiVersion)
                .Replace("{TableName}", modelName);
            FileUtils.WriteFile(serverPath, modelName + "Service.cs", treeServiceString);
        }

        #region 前端

        //JS
        var jsString = jsTemp.Replace("{TableName}", modelName.ToLower());
        FileUtils.WriteFile(path + "Generator/Vue/", modelName.ToLower() + ".js", jsString);

        //List
        string webTableColumnStr = string.Empty, formColumnStr = string.Empty, formData = string.Empty
            ,rules=string.Empty;
        var optionsString = new StringBuilder();
        if (createModel.IsGrid)
        {
            formColumnStr = "<el-row>";
        }
        foreach (var item in createModel.TableColumnInfo)
        {
            //列
            if (item.IsColumn)
            {
                webTableColumnStr += "                { prop: '" + item.DbColumnName.FirstCharToLower() + "', label: '" +
                                     item.ColumnDescription + "', width: 100 },\r\n";
            }
            
            if (!item.IsAdd) continue;

            if (createModel.IsGrid)
            {
                formColumnStr += "<el-col :span=\"12\"> \r\n";
            }
            if (item.ComponentType is "input" or "textarea")
            {
                formColumnStr += "<el-form-item label=\"" + item.ColumnDescription + "\" prop=\"" +
                                 item.DbColumnName.FirstCharToLower() + "\"> \r\n";
                formColumnStr += "	<el-input \r\n";
                formColumnStr += "		v-model=\"formData." + item.DbColumnName.FirstCharToLower() + "\" \r\n";
                if (item.ComponentType == "textarea")
                {
                    formColumnStr += "		type=\"textarea\" \r\n";
                }
                formColumnStr += "		placeholder=\"请输入" + item.ColumnDescription + "\" \r\n";
                formColumnStr += "		:maxlength=\"" + item.Length + "\" \r\n";
                formColumnStr += "		show-word-limit \r\n";
                formColumnStr += "		clearable \r\n";
                formColumnStr += "	></el-input> \r\n";
                formColumnStr += "</el-form-item> \r\n";
            }

            if (item.ComponentType=="switch")
            {
                formColumnStr += "<el-form-item label=\"" + item.ColumnDescription + "\" prop=\"" +
                                 item.DbColumnName.FirstCharToLower() + "\">\r\n";
                formColumnStr += "	<el-switch \r\n";
                formColumnStr += "		v-model=\"formData." + item.DbColumnName.FirstCharToLower() + "\" \r\n";
                formColumnStr += "	></el-switch> \r\n";
                formColumnStr += "</el-form-item> \r\n";
            }
            if (item.ComponentType=="time")
            {
                formColumnStr += "<el-form-item label=\"" + item.ColumnDescription + "\" prop=\"" +
                                 item.DbColumnName.FirstCharToLower() + "\">\r\n";
                formColumnStr += "	<el-date-picker \r\n";
                formColumnStr += "		v-model=\"formData." + item.DbColumnName.FirstCharToLower() + "\" \r\n";
                formColumnStr += ":style=\"{ width: '100%' }\"";
                formColumnStr += "	placeholder=\"请输入"+item.ColumnDescription+"\" \r\n";
                formColumnStr += "	clearable \r\n";
                formColumnStr += "></el-date-picker> \r\n";
                formColumnStr += "</el-form-item> \r\n";
            }
            if (item.ComponentType=="select")
            {
                formColumnStr += "<el-form-item label=\"" + item.ColumnDescription + "\" prop=\"" +
                                 item.DbColumnName.FirstCharToLower() + "\">\r\n";
                formColumnStr += "	<el-select \r\n";
                formColumnStr += "		v-model=\"formData." + item.DbColumnName.FirstCharToLower() + "\" \r\n";
                formColumnStr += "	placeholder=\"请选择"+item.ColumnDescription+"\" clearable > \r\n";
                formColumnStr += "	<el-option \r\n";
                formColumnStr += "	v-for=\"item in "+item.DbColumnName.FirstCharToLower()+"Options\" \r\n";
                formColumnStr += "	:key=\"item.value\" \r\n";
                formColumnStr += "	:label=\"item.label\" \r\n";
                formColumnStr += "	:value=\"item.value\" /> \r\n";
                formColumnStr += "</el-select> \r\n";
                formColumnStr += "</el-form-item> \r\n";

                optionsString.Append(item.DbColumnName.FirstCharToLower()+"Options:[],");
            }
            if (createModel.IsGrid)
            {
                formColumnStr += "</el-col> \r\n";
            }
            formData += item.DbColumnName.FirstCharToLower() + ":"+(item.ComponentType=="switch"?"true":"undefined")+", \r\n";
            if (!item.IsNullable)
            {
                rules += item.DbColumnName.FirstCharToLower()+": [{required: true,message: \"请输入"+item.ColumnDescription+"\",trigger: \""+(item.ComponentType is "select" or "time"?"change":"blur")+"\",},],";
            }

        }
        if (createModel.IsGrid)
        {
            formColumnStr += "</el-row>";
        }
        var listString = listTemp.Replace("{TableName}", modelName.ToLower())
            .Replace("{NameSpace}", createModel.Namespace.ToLower())
            .Replace("{Column}", webTableColumnStr);
        FileUtils.WriteFile(path + "Generator/Vue/" + createModel.Namespace + "/" + modelName + "/", "index.vue", listString);

        
        //Modify
        var modifyString = modifyTemp.Replace("{TableName}",modelName.ToLower())
            .Replace("{NameSpace}",createModel.Namespace.ToLower())
            .Replace("{TableColumn}",formColumnStr)
            .Replace("{formData}",formData)
            .Replace("[RULES]",rules)
            .Replace("{options}",optionsString.ToString())
            .Replace("{TableNameDescribe}",table.Description);
        FileUtils.WriteFile(path+"Generator/Vue/"+createModel.Namespace+"/"+modelName+"/",  "modify.vue", modifyString);
        
        #endregion

        return path;
    }

    /// <summary>
    /// 连接数据库，并返回当前连接下所有表名字
    /// </summary>
    /// <returns></returns>
    public List<DbTableInfo> InitTable()
    {
        return Context.DbMaintenance.GetTableInfoList();
    }

    /// <summary>
    /// 根据表名查询列信息
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public List<DbColumnInfo> GetColumn(string tableName)
    {
        return Context.DbMaintenance.GetColumnInfosByTableName(tableName);
    }
}