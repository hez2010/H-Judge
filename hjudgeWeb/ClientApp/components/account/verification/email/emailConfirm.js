import { Post } from '../../../../utilities/requestHelper';

export default {
    props: ['user', 'getUserInfo', 'closeDlg'],
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
                            this.getUserInfo();
                            this.closeDlg();
                        }
                        else alert(data.errorMessage);
                        this.submitting = false;
                    })
                    .catch(() => {
                        alert('请求失败');
                        this.submitting = false;
                    });
            }
        }
    }
};