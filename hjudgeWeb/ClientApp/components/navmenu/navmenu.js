import login from '../account/login/login.vue';
import register from '../account/register/register.vue';
import { Post } from '../../utilities/requestHelper';

export default {
    props: ['user', 'themeIcon', 'switchTheme', 'getUserInfo'],
    data: () => ({
        drawer: null,
        loginDialog: false,
        registerDialog: false,
        items: [
            { heading: 'H::Judge' },
            { icon: 'home', text: '主页', link: '/' },
            { icon: 'code', text: '题目', link: '/Problem' },
            { icon: 'access_time', text: '比赛', link: '/Contest' },
            { icon: 'group', text: '小组', link: '/Group' },
            { icon: 'list', text: '状态', link: '/Status' },
            { icon: 'help', text: '关于', link: '/About' }
        ],
        msgTitle: '',
        msgContent: '',
        msgUp: false
    }),
    mounted: function () {
        this.items.splice(7);
        if (this.user && this.user.isSignedIn) {
            this.items = this.items.concat([{ icon: 'message', text: '消息', link: '/Message' }]);
            if (this.user.privilege === 1) {
                this.items = this.items.concat([{ icon: 'settings', text: '设置', link: '/Admin/Config' }]);
            }
        }
    },
    watch: {
        user: function () {
            this.items.splice(7);
            if (this.user && this.user.isSignedIn) {
                this.items = this.items.concat([{ icon: 'message', text: '消息', link: '/Message' }]);
                if (this.user.privilege === 1) {
                    this.items = this.items.concat([{ icon: 'settings', text: '设置', link: '/Admin/Config' }]);
                }
            }
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
            sessionStorage.clear();
            Post('/Account/Logout')
                .then(() => this.getUserInfo());
        },
        goback: function () {
            this.$router.go(-1);
        },
        closeDlg: function () {
            this.loginDialog = false;
            this.registerDialog = false;
        },
        showMsg: function (title, content) {
            this.msgTitle = title;
            this.msgContent = content;
            this.msgUp = true;
        }
    },
    components: {
        login: login,
        register: register
    }
};