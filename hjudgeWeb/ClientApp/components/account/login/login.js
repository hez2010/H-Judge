import { Post } from '../../../utilities/requestHelper';
import resetpassword from '../password/password.vue';

export default {
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
                        if (data && data.isSucceeded) {
                            window.location.reload();
                        }
                        else {
                            alert(data.errorMessage);
                            this.submitting = false;
                        }
                    })
                    .catch(() => {
                        alert('登录失败');
                        this.submitting = false;
                    });
            }
        },
        closeResetDialog: function () {
            this.resetDialog = false;
        }
    },
    components: {
        resetpassword: resetpassword
    }
};