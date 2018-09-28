import { Post } from '../../../utilities/requestHelper';

export default {
    data: () => ({
        valid: false,
        username: '',
        usernameRules: [
            v => !!v || '请输入用户名',
            v => (v && v.length <= 10) || '用户名长度不能大于 10'
        ],
        password: '',
        passwordRules: [
            v => !!v || '请输入密码'
        ]
    }),
    methods: {
        login: function () {
            if (this.$refs.form.validate()) {
                Post('/Account/Login', { username: this.username, password: this.password })
                    .then(res => res.json())
                    .then(data => {
                        if (data && data.isSucceeded) {
                            window.location = '/';
                        }
                        else {
                            alert(data.errorMessage);
                        }
                    })
                    .catch(() => alert('登录失败'));
            }
        }
    }
};