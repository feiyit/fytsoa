namespace FytSoa.Application.Sys;

public class SysMenuDropDto
{
    /// <summary>
    /// 拖动节点
    /// </summary>
    public SysMenuDropModelDto DraggingNode { get; set; }
    
    /// <summary>
    /// 跌落节点
    /// </summary>
    public SysMenuDropModelDto DropNode { get; set; }
    
    /// <summary>
    /// 模式   before 向上  after 向下  inner 变更为子级
    /// </summary>
    public string DropType { get; set; }
    
    /// <summary>
    /// 拖拽对象
    /// </summary>
    public class SysMenuDropModelDto
    {
        public long Id { get; set; }
        
        public string Label { get; set; }
        
        public long ParentId { get; set; }
    }
}