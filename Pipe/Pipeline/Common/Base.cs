using System;
using System.Collections.Generic;
using System.Text;
using NorthStar.Common.Log;

namespace NorthStar.Common
{
    /********************************************************************************

    ** 类名称： Base
    ** 描述：
    ** 作者： 
    ** 创建时间： 2017/7/27 15:38:24
    ** 最后修改人：（无）
    ** 最后修改时间：2017/7/27 15:38:24
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/
    [Loggable]
    [PointCut("public void *(*)",false)]
    public class Base : ContextBoundObject
    {

    }
}
