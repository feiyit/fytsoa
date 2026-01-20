export const customTemplates = [
	{
		title: '字母标题',
		description: '字母标题，渐变高级感，带图标',
		content: `
		<div class="editor-cur-header">
                            <div class="editor-cur-icon">A</div>
                            <div class="editor-cur-title">分类标题</div>
                        </div>
	  `,
		categories: ['基本']
	},
	{
		title: '分类卡片内容一',
		description: '分类卡片内容，带图标，带标题',
		content: `
		<div class="editor-cur-card-a">
		<div class="type-item">
            <h4>1. 标题</h4>
            <p>内容系列。<span class="highlight">高亮</span>，更多内容。</p>
            <div class="brands">
                <span class="brand-tag">标签一</span>
                <span class="brand-tag">标签二</span>
                <span class="brand-tag">标签三</span>
            </div>
        </div>
        </div>
	  `,
		categories: ['基本']
	},
	{
		title: '分类卡片内容二',
		description: '分类卡片内容，带图标，带标题',
		content: `
		<div class="editor-cur-card-b">
		<div class="type-item">
            <h4>1. 标题</h4>
            <p>内容系列。<span class="highlight">高亮</span>，更多内容。</p>
            <div class="brands">
                <span class="brand-tag">标签一</span>
                <span class="brand-tag">标签二</span>
                <span class="brand-tag">标签三</span>
            </div>
        </div>
        </div>
	  `,
		categories: ['基本']
	},
	{
		title: '分类卡片内容三',
		description: '分类卡片内容，支持多级标题',
		content: `
		<div class="editor-cur-card-c">
			<h3>1.1 温度分类体系</h3>
			<h4>1.1.1 按温标体系分类</h4>
			<div class="type-item">
				<h4>1.1.1.1 标题</h4>
            	<p>内容系列。<span class="highlight">高亮</span>，更多内容。</p>
        	</div>
        </div>
	  `,
		categories: ['基本']
	},
	{
		title: '分类卡片内容四',
		description: '分类卡片内容，支持多级标题，纯白色',
		content: `
		<div class="editor-cur-card-c editor-cur-card-d">
			<h3>1.1 温度分类体系</h3>
			<h4>1.1.1 按温标体系分类</h4>
			<div class="type-item">
				<h4>1.1.1.1 标题</h4>
            	<p>内容系列。<span class="highlight">高亮</span>，更多内容。</p>
        	</div>
        </div>
	  `,
		categories: ['基本']
	},
	{
		title: '提示框',
		description: '蓝色提示框样式',
		content: `
		<div class="template-alert">
		  <p>这是一个重要提示！</p>
		</div>
	  `,
		categories: ['提示']
	},
	{
		title: '对比列表',
		description: '左右对比的双栏布局',
		content: `
		<div class="template-comparison">
		  <div class="template-col">
			<h3>优势</h3>
			<ul>
			  <li>不同类型的新风系统适用场景不同</li>
			  <li>选择时需结合房屋状况</li>
			  <li>当地气候和实际需求</li>
			  <li>确保系统匹配您的具体需求</li>
			</ul>
		  </div>
		  <div class="template-col">
			<h3>改进</h3>
			<ul>
			  <li>改进点一</li>
			  <li>改进点二</li>
			</ul>
		  </div>
		</div>
	  `,
		categories: ['布局']
	}
]
