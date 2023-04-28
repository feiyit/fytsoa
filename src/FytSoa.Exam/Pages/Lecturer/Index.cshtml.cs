using FytSoa.Application.Exam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FytSoa.Exam.Pages.Lecturer;

public class IndexModel : PageModel
{
    private readonly ExamTeacherService _teacherService;
    public IndexModel(ExamTeacherService teacherService)
    {
        _teacherService = teacherService;
    }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAddFocus([FromBody]FocusParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new{Content ="参数验证失败~",StatusCode = 500}) ;
        }
        await _teacherService.ModifyAddFocusAsync(param.Id);
        return new JsonResult(new{StatusCode = 200});
    }

    public class FocusParam
    {
        public long Id { get; set; }
    }
}