import * as React from "react";
import { Header, Icon, Item, Label, Divider } from "semantic-ui-react";
import { setTitle } from "../../utils/titleHelper";

interface AboutState {
  upgradeLogs: UpgradeLog[]
}

interface UpgradeLog {
  version: string,
  date: string,
  content: string[]
}

export default class About extends React.Component<{}, AboutState> {
  constructor(props: {}) {
    super(props);

    this.state = {
      upgradeLogs: [{
        version: 'Beta: v1.7',
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
        version: 'Beta: v1.6',
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
        version: 'Beta: v1.5',
        date: '2018/11/15',
        content: [
          '1. 添加版聊功能',
          '2. 重新设计关于界面',
          '3. 重新设计主页',
          '4. 开放重新评测权限'
        ]
      },
      {
        version: 'Stable: v1.4',
        date: '2018/10/21',
        content: [
          '1. 添加题目通过比率',
          '2. 添加菜单按钮提示',
          '3. 支持为单独题目设置语言限制'
        ]
      },
      {
        version: 'Stable: v1.3',
        date: '2018/10/20',
        content: [
          '1. 添加邮箱验证功能',
          '2. 优化用户体验，全部操作无需重新载入页面',
          '3. 添加 CSRF 攻击防御'
        ]
      },
      {
        version: 'Stable: v1.2',
        date: '2018/10/09',
        content: [
          '1. 添加密码重置功能'
        ]
      },
      {
        version: 'Stable: v1.1',
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
        version: 'Stable: v1.0',
        date: '2018/10/05',
        content: [
          '1. 排名功能上线',
          '2. 添加小组和消息功能占位',
          '3. H:: Judge 正式版来啦'
        ]
      },
      {
        version: 'Beta: v0.4',
        date: '2018/10/04',
        content: [
          '1. 核心功能基本完成',
          '2. 添加夜间模式',
          '3. Bug 修复'
        ]
      },
      {
        version: 'Alpha: v0.3',
        date: '2018/10/03',
        content: [
          '1. 管理功能全部完成',
          '2. Bug 修复'
        ]
      },
      {
        version: 'Alpha: v0.2',
        date: '2018/10/02',
        content: [
          '1. 比赛功能上线',
          '2. 引入新的评测机',
          '3. Bug 修复'
        ]
      },
      {
        version: 'Alpha: v0.1',
        date: '2018/10/01',
        content: [
          '1. 船新的 H::Judge 建造出来啦~'
        ]
      }
      ]
    }
  }

  componentDidMount() {
    setTitle('关于');
  }
  render() {
    return (
      <>
        <Header as='h1'>H::Judge</Header>
        <p>这是你从未上过的船新 H::Judge</p>
        <div>
          <Label>开发者</Label> <a target='_blank' href='https://hez2010.com'>hez2010</a>
          <Label>Email</Label> <a target='_blank' href='mailto:hez2010@outlook.com'>hez2010@outlook.com</a>
          <Label>GitHub</Label> <a target='_blank' href='https://github.com/hez2010/H-Judge'>hez2010/H-Judge</a>
        </div>
        <div>
          <Divider horizontal>
            <Header as='h4'>
              <Icon name='tag'></Icon> 更新日志
            </Header>
          </Divider>
          <Item.Group>
            {
              this.state.upgradeLogs.map((v, i) =>
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
}