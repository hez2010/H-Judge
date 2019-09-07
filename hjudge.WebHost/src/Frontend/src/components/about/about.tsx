import * as React from 'react';
import { Header, Icon, Item, Divider } from 'semantic-ui-react';
import { setTitle } from '../../utils/titleHelper';

const About = () => {
  const upgradeLogs = [{
    version: 'Beta: 2.0-preview9',
    date: '2019/09/07',
    content: [
      '1. 修复相同排名顺序 bug',
      '2. 添加提交量和通过量统计',
      '3. 添加加载动画',
      '4. 更多优化',
    ]
  },
  {
    version: 'Alpha: 2.0-preview8',
    date: '2019/09/05',
    content: [
      '1. 初步添加小组支持',
      '2. 主页添加动态 Hub',
      '3. 更新比赛排名算法',
      '4. 修复评测器内存测量错误的 bug',
    ]
  },
  {
    version: 'Alpha: 2.0-preview7',
    date: '2019/08/27',
    content: [
      '1. 改进状态结果筛选器',
      '2. 修复题目状态值错误的 bug'
    ]
  },
  {
    version: 'Alpha: 2.0-preview6',
    date: '2019/08/26',
    content: [
      '1. 修复服务端渲染适配',
      '2. 修复 SPJ 评测支持'
    ]
  },
  {
    version: 'Alpha: 2.0-preview5',
    date: '2019/08/24',
    content: [
      '1. 完成评测结果页',
      '2. 完成排名系统',
      '3. 实时推送评测结果'
    ]
  },
  {
    version: 'Alpha: 2.0-preview4',
    date: '2019/08/16',
    content: [
      '1. 完成比赛详情页',
      '2. 完成题目配置和比赛配置功能',
      '3. 支持数据上传'
    ]
  },
  {
    version: 'Alpha: 2.0-preview3',
    date: '2019/06/18',
    content: [
      '1. 完成新账户门户页面',
      '2. 支持分布式文件系统',
      '3. 完成题目详情页'
    ]
  },
  {
    version: 'Alpha: 2.0-preview2',
    date: '2019/06/04',
    content: [
      '1. 启用服务端渲染加速首屏展示',
      '2. 切换至 PostgreSQL 数据库',
      '3. 完成新题目列表、比赛列表和小组列表展示'
    ]
  },
  {
    version: 'Alpha: 2.0-preview1',
    date: '2019/05/28',
    content: [
      '1. 完全重构',
      '2. 后端更新至 .NET Core 3.0',
      '3. 前端使用 TypeScript 并从 Vue 切换到 React',
      '4. 支持分布式评测机'
    ]
  },
  {
    version: 'Stable: 1.9',
    date: '2019/04/17',
    content: [
      '1. 比赛题目列表 bug 修复'
    ]
  },
  {
    version: 'Beta: 1.7',
    date: '2018/11/28',
    content: [
      '1. 全部板块上线讨论功能',
      '2. 部分消息功能上线',
      '3. 性能优化',
      '4. 评测机改进',
      '5. 其他细节优化'
    ]
  },
  {
    version: 'Beta: 1.6',
    date: '2018/11/21',
    content: [
      '1. 优化 UI',
      '2. 改善用户体验',
      '3. 发送消息获得经验',
      '4. 添加每日登录奖励',
      '5. 编辑器支持 Markdown',
      '6. 性能优化',
      '7. 添加代码编辑器和语法高亮'
    ]
  },
  {
    version: 'Beta: 1.5',
    date: '2018/11/15',
    content: [
      '1. 添加版聊功能',
      '2. 重新设计关于界面',
      '3. 重新设计主页',
      '4. 开放重新评测权限'
    ]
  },
  {
    version: 'Stable: 1.4',
    date: '2018/10/21',
    content: [
      '1. 添加题目通过比率',
      '2. 添加菜单按钮提示',
      '3. 支持为单独题目设置语言限制'
    ]
  },
  {
    version: 'Stable: 1.3',
    date: '2018/10/20',
    content: [
      '1. 添加邮箱验证功能',
      '2. 优化用户体验，全部操作无需重新载入页面',
      '3. 添加 CSRF 攻击防御'
    ]
  },
  {
    version: 'Stable: 1.2',
    date: '2018/10/09',
    content: [
      '1. 添加密码重置功能'
    ]
  },
  {
    version: 'Stable: 1.1',
    date: '2018/10/08',
    content: [
      '1. 修复后退页面列表不会更新的 Bug',
      '2. 更换页面跳转实现方式',
      '3. 兼容 IE 11',
      '4. 评测逻辑 Bug 修复',
      '5. 其他优化'
    ]
  },
  {
    version: 'Stable: 1.0',
    date: '2018/10/05',
    content: [
      '1. 排名功能上线',
      '2. 添加小组和消息功能占位',
      '3. H:: Judge 正式版来啦'
    ]
  },
  {
    version: 'Beta: 1.0-preview4',
    date: '2018/10/04',
    content: [
      '1. 核心功能基本完成',
      '2. 添加夜间模式',
      '3. Bug 修复'
    ]
  },
  {
    version: 'Alpha: 1.0-preview3',
    date: '2018/10/03',
    content: [
      '1. 管理功能全部完成',
      '2. Bug 修复'
    ]
  },
  {
    version: 'Alpha: 1.0-preview2',
    date: '2018/10/02',
    content: [
      '1. 比赛功能上线',
      '2. 引入新的评测机',
      '3. Bug 修复'
    ]
  },
  {
    version: 'Alpha: 1.0-preview1',
    date: '2018/10/01',
    content: [
      '1. 船新的 H::Judge 建造出来啦~'
    ]
  }];

  setTitle('关于');

  return (
    <>
      <Header as='h1'>H::Judge</Header>
      <p>这是你从未上过的船新 H::Judge</p>
      <div>
        <Divider horizontal>
          <Header as='h4'>
            <Icon name='tag'></Icon> 更新日志
            </Header>
        </Divider>
        <Item.Group>
          {
            upgradeLogs.map((v, i) =>
              <React.Fragment key={i}>
                <Item>
                  <Item.Content>
                    <Item.Header>{v.version}</Item.Header>
                    <Item.Meta>{v.date}</Item.Meta>
                    <Item.Description>
                      {v.content.map((vc, ic) => <p key={ic}>{vc}</p>)}
                    </Item.Description>
                  </Item.Content>
                </Item>
                <Divider />
              </React.Fragment>
            )
          }
        </Item.Group>
      </div>
    </>
  );
}

export default About;