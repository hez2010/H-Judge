import { Post } from '../../../../utilities/requestHelper';

export default {
    props: ['user', 'getUserInfo', 'closeDlg', 'showSnack'],
    data: () => ({
        valid: false,
        token: '',
        tokenRules: [
            v => !!v || '请输入验证码'
        ],
        submitting: false
    }),
    methods: {
        clearForm: function () {
            this.token = '';
        },
        confirmEmail: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                Post('/Account/ConfirmEmail', { token: this.token })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('邮箱地址验证成功', 'success', 3000);
                            this.getUserInfo();
                            this.closeDlg();
                        }
                        else
                            this.showSnack(data.errorMessage, 'error', 3000);
                        this.submitting = false;
                    })
                    .catch(() => {
                        this.showSnack('请求失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        }
    }
};