using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.Runtime;

namespace FSVN.Server
{
    /// <summary>
    /// 工作流宿主环境实现接口
    /// </summary>
	public interface IWorkflowRuntimeHost
	{
        /// <summary>
        /// 获取工作流运行时环境
        /// </summary>
        /// <returns></returns>
        WorkflowRuntime GetRuntime();

	}
}
