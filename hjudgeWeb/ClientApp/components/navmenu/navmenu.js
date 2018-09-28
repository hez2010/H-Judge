import login from '../account/login/login.vue';
import register from '../account/register/register.vue';
import { Post } from '../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        drawer: null,
        loginDialog: false,
        items: [
            { icon: 'home', text: '主页', link: '/' },
            { icon: 'code', text: '题目', link: '/Problem' },
            { icon: 'access_time', text: '比赛', link: '/Contest' },
            { icon: 'group', text: '小组', link: '/Group' },
            { icon: 'list', text: '状态', link: '/Status' },
            { icon: 'help', text: '关于', link: '/About' }
        ]
    }),
    watch: {
        user: function () {
            if (this.user !== null) {
                if (this.user.privilege >= 1 && this.user.privilege <= 3) {
                    this.items = this.items.concat([{
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
                    }]);
                }
                else {
                    if (this.items[6] !== null) {
                        this.items = this.items.splice(6, 1);
                    }
                }
            }
        }
    },
    methods: {
        logout: function () {
            Post('/Account/Logout')
                .then(() => window.location = '/');
        }
    },
    components: {
        login: login,
        register: register
    }
};