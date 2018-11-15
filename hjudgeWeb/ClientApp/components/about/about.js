import { setTitle } from '../../utilities/titleHelper';

export default {
    data: () => ({
        logs: [
            {
                color: 'indigo',
                version: 'Beta: v1.5',
                reldate: '2018/11/15',
                content: [
                    '1. 添加版聊功能',
                    '2. 重新设计关于界面',
                    '3. 重新设计主页',
                    '4. 开放重新评测权限'
                ]
            },
            {
                color: 'cyan',
                version: 'Stable: v1.4',
                reldate: '2018/10/21',
                content: [
                    '1. 添加题目通过比率',
                    '2. 添加菜单按钮提示',
                    '3. 支持为单独题目设置语言限制'
                ]
            },
            {
                color: 'green',
                version: 'Stable: v1.3',
                reldate: '2018/10/20',
                content: [
                    '1. 添加邮箱验证功能',
                    '2. 优化用户体验，全部操作无需重新载入页面',
                    '3. 添加 CSRF 攻击防御'
                ]
            },
            {
                color: 'pink',
                version: 'Stable: v1.2',
                reldate: '2018/10/09',
                content: [
                    '1. 添加密码重置功能'
                ]
            },
            {
                color: 'amber',
                version: 'Stable: v1.1',
                reldate: '2018/10/08',
                content: [
                    '1. 修复后退页面列表不会更新的 Bug',
                    '2. 更换页面跳转实现方式',
                    '3. 兼容 IE 11',
                    '4. 评测逻辑 Bug 修复',
                    '5. 其他优化'
                ]
            },
            {
                color: 'orange',
                version: 'Stable: v1.0',
                reldate: '2018/10/05',
                content: [
                    '1. 排名功能上线',
                    '2. 添加小组和消息功能占位',
                    '3. H:: Judge 正式版来啦'
                ]
            },
            {
                color: 'grey',
                version: 'Beta: v0.4',
                reldate: '2018/10/04',
                content: [
                    '1. 核心功能基本完成',
                    '2. 添加夜间模式',
                    '3. Bug 修复'
                ]
            },
            {
                color: 'blue',
                version: 'Alpha: v0.3',
                reldate: '2018/10/03',
                content: [
                    '1. 管理功能全部完成',
                    '2. Bug 修复'
                ]
            },
            {
                color: 'purple',
                version: 'Alpha: v0.2',
                reldate: '2018/10/02',
                content: [
                    '1. 比赛功能上线',
                    '2. 引入新的评测机',
                    '3. Bug 修复'
                ]
            },
            {
                color: 'red',
                version: 'Alpha: v0.1',
                reldate: '2018/10/01',
                content: [
                    '1. 船新的 H::Judge 建造出来啦~'
                ]
            }
        ]
    }),
    mounted: function () {
        setTitle('关于');
    }
};