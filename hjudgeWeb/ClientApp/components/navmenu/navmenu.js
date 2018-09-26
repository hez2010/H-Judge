export default {
    data: {
        drawer: null,
        items: [
            { icon: 'home', text: '主页', link: '/' },
            { icon: 'code', text: '题目', link: '/Problem' },
            { icon: 'access_time', text: '比赛', link: '/Contest' },
            { icon: 'list', text: '状态', link: '/Status' },
            { icon: 'group', text: '小组', link: '/Group' },
            {
                icon: 'keyboard_arrow_up',
                'icon-alt': 'keyboard_arrow_down',
                text: '管理',
                model: false,
                children: [
                    { icon: 'code', text: '题目管理', link: '/Admin/Problem' },
                    { icon: 'access_time', text: '比赛管理', link: '/Admin/Contest' },
                    { icon: 'group', text: '小组管理', link: '/Admin/Group' },
                    { icon: 'person', text: '用户管理', link: '/Admin/User' },
                    { icon: 'settings', text: '系统设置', link: '/Admin/Config' }
                ]
            },
            { icon: 'help', text: '关于', link: '/About' }
        ]
    },
    methods: {
        test: function () {
            this.$emit('register');
        }
    }
};