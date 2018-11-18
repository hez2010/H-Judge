import { Post } from '../../../utilities/requestHelper';
import resetpassword from '../password/password.vue';

export default {
    props: ['getUserInfo', 'closeDlg', 'showSnack'],
    data: () => ({
        valid: false,
        submitting: false,
        username: '',
        usernameRules: [
            v => !!v || '请输入用户名',
            v => (v && v.length <= 30) || '用户名长度不能大于 30'
        ],
        password: '',
        passwordRules: [
            v => !!v || '请输入密码'
        ],
        rememberMe: true,
        resetDialog: false
    }),
    watch: {
        resetDialog: function () {
            this.$refs.resetpwd.clearForm();
        }
    },
    methods: {
        login: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                Post('/Account/Login', { username: this.username, password: this.password, rememberMe: this.rememberMe })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('登录成功', 'success', 3000);
                            this.getUserInfo();
                            this.closeDlg();
                        }
                        else this.showSnack(data.errorMessage, 'error', 3000);
                        this.submitting = false;
                    })
                    .catch(() => {
                        this.showSnack('登录失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        },
        closeResetDialog: function () {
            this.resetDialog = false;
        },
        clearForm: function () {
            this.username = '';
            this.password = '';
            this.rememberMe = true;
            this.resetDialog = false;
        }
    },
    components: {
        resetpassword: resetpassword
    }
};