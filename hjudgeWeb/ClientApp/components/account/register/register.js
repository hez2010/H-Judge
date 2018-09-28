import { Post } from '../../../utilities/requestHelper';

export default {
    data: () => ({
        valid: false,
        submitting: false,
        username: '',
        usernameRules: [
            v => !!v || '请输入用户名',
            v => (v && v.length <= 10) || '用户名长度不能大于 10'
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
                        if (data && data.isSucceeded) {
                            window.location = '/';
                        }
                        else {
                            alert(data.errorMessage);
                            this.submitting = false;
                        }
                    })
                    .catch(() => {
                        alert('注册失败');
                        this.submitting = false;
                    });
            }
        },
        passwordChanged: function () {
            this.confirmPasswordRules = [
                v => !!v || '请输入确认密码',
                v => v === this.password || '两次输入的密码不一致'
            ];
        }
    }
};