using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.ComponentModel;

namespace FSVN.Activities
{
    /// <summary>
    /// 转移文件活动
    /// </summary>
    public class FileMoveActivity : Activity
    {

        /// <summary>
        /// 由工作流运行时调用，用于执行活动。
        /// </summary>
        /// <param name="executionContext">与此 <see cref="T:System.Workflow.ComponentModel.Activity"/> 和执行关联的 <see cref="T:System.Workflow.ComponentModel.ActivityExecutionContext"/>。</param>
        /// <returns>
        /// 运行任务的 <see cref="T:System.Workflow.ComponentModel.ActivityExecutionStatus"/>，确定活动是保留执行状态，还是转换为关闭状态。
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return base.Execute(executionContext);
        }

        private void InitializeComponent()
        {
            // 
            // FileMoveActivity
            // 
            this.Description = "文件移动活动块";
            this.Name = "FileMoveActivity";

        }

    }
}
