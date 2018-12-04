const login = () => import(/* webpackChunkName: 'login' */'../account/login/login.vue');
const register = () => import(/* webpackChunkName: 'register' */'../account/register/register.vue');
import { Get, Post } from '../../utilities/requestHelper';

export default {
    props: ['user', 'themeIcon', 'switchTheme', 'getUserInfo', 'showSnack'],
    data: () => ({
        drawer: null,
        loginDialog: false,
        registerDialog: false,
        items: [
            { heading: 'H::Judge' },
            { icon: 'home', text: '主页', link: '/', badge: 0 },
            { icon: 'code', text: '题目', link: '/Problem', badge: 0 },
            { icon: 'access_time', text: '比赛', link: '/Contest', badge: 0 },
            { icon: 'group', text: '小组', link: '/Group', badge: 0 },
            { icon: 'list', text: '状态', link: '/Status', badge: 0 },
            { icon: 'help', text: '关于', link: '/About', badge: 0 }
        ]
    }),
    mounted: function () {
        this.loadPersonalMenu();
    },
    watch: {
        user: function () {
            this.loadPersonalMenu();
        },
        loginDialog: function () {
            this.$refs.loginDlg.clearForm();
        },
        registerDialog: function () {
            this.$refs.registerDlg.clearForm();
        }
    },
    methods: {
        logout: function () {
            Post('/Account/Logout')
                .then(() => {
                    sessionStorage.clear();
                    this.getUserInfo();
                    this.$router.push('/');
                    this.showSnack('退出成功', 'success', 3000);
                })
                .catch(() => {
                    this.showSnack('退出失败', 'error', 3000);
                });
        },
        goback: function () {
            this.$router.go(-1);
        },
        closeDlg: function () {
            this.loginDialog = false;
            this.registerDialog = false;
        },
        loadPersonalMenu: function () {
            this.items.splice(7);
            if (this.user && this.user.isSignedIn) {
                this.items = this.items.concat([{ icon: 'message', text: '消息', link: '/Message', badge: 0 }]);
                if (this.user.privilege === 1) {
                    this.items = this.items.concat([{ icon: 'settings', text: '设置', link: '/Admin/Config', badge: 0 }]);
                }
            }
        },
        //mode: 1 -- absolute, 2 -- relative
        setMessageCount: function (mode, value) {
            if (!(this.user && this.user.isSignedIn && this.items.length > 7)) return;
            switch (mode) {
                case 1:
                    this.items[7].badge = value;
                    break;
                case 2:
                    this.items[7].badge += value;
                    break;
            }
        }
    },
    components: {
        login: login,
        register: register
    }
};
