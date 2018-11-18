import { Post } from '../../../utilities/requestHelper';

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
        email: '',
        emailRules: [
            v => !!v || '请输入邮箱地址',
            v => /.+@.+\..+/.test(v) || '邮箱地址格式不正确'
        ],
        password: '',
        passwordRules: [
            v => !!v || '请输入密码'
        ],
        confirmPassword: '',
        confirmPasswordRules: [
            v => !!v || '请输入确认密码'
        ]
    }),
    methods: {
        register: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                Post('/Account/Register', { username: this.username, email: this.email, password: this.password, confirmPassword: this.confirmPassword })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('注册成功', 'success', 3000);
                            this.getUserInfo();
                            this.closeDlg();
                        }
                        else this.showSnack(data.errorMessage, 'error', 3000);
                        this.submitting = false;
                    })
                    .catch(() => {
                        this.showSnack('注册失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        },
        passwordChanged: function () {
            this.confirmPasswordRules = [
                v => !!v || '请输入确认密码',
                v => v === this.password || '两次输入的密码不一致'
            ];
        },
        clearForm: function () {
            this.username = '';
            this.password = '';
            this.confirmPassword = '';
            this.email = '';
        }
    }
};