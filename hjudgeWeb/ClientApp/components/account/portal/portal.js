import { setTitle } from '../../../utilities/titleHelper';
import { Get, Post, ReadCookie } from '../../../utilities/requestHelper';
import emailConfirm from '../verification/email/emailConfirm.vue';
import phoneConfirm from '../verification/phone/phoneConfirm.vue';

export default {
    props: ['user', 'getUserInfo'],
    data: () => ({
        avatar: '',
        bottomNav: '1',
        privilege: '',
        emailRules: [
            v => !!v || '请输入邮箱地址',
            v => /.+@.+\..+/.test(v) || '邮箱地址格式不正确'
        ],
        confirmEmailDialog: false,
        submitting: false
    }),
    mounted: function () {
        setTitle('门户');
        if (this.user !== null) {
            this.privilege = this.user.privilege === 1 ? '管理员' :
                this.user.privilege === 2 ? '教师' :
                    this.user.privilege === 3 ? '助教' :
                        this.user.privilege === 4 ? '学生/选手' :
                            this.user.privilege === 5 ? '黑名单' : '未知';
            Get('/Account/GetUserAvatar')
                .then(res => res.text())
                .then(data => this.avatar = 'data:image/png;base64, ' + data);
        }
    },
    watch: {
        user: function () {
            if (this.user !== null) {
                this.privilege = this.user.privilege === 1 ? '管理员' :
                    this.user.privilege === 2 ? '教师' :
                        this.user.privilege === 3 ? '助教' :
                            this.user.privilege === 4 ? '学生/选手' :
                                this.user.privilege === 5 ? '黑名单' : '未知';
                Get('/Account/GetUserAvatar')
                    .then(res => res.text())
                    .then(data => this.avatar = 'data:image/png;base64, ' + data);
            }
        },
        confirmEmailDialog: function () {
            this.$refs.confirmEmailDlg.clearForm();
        }
    },
    computed: {
        isColumn: function () {
            let binding = { column: true };
            if (this.$vuetify.breakpoint.mdAndUp)
                binding.column = false;
            return binding;
        },
        isColumnR: function () {
            let binding = { column: false };
            if (this.$vuetify.breakpoint.mdAndUp)
                binding.column = true;
            return binding;
        }
    },
    methods: {
        updateOtherInfo: function () {
            let otherInfo = {};
            for (var item in this.user.otherInfo) {
                otherInfo[this.user.otherInfo[item].key] = this.user.otherInfo[item].value;
            }
            Post('/Account/UpdateOtherInfo', otherInfo)
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded)
                        alert(data.errorMessage);
                })
                .catch(() => alert('修改失败'));
        },
        updateName: function (name) {
            Post('/Account/UpdateName', { value: name })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded)
                        alert(data.errorMessage);
                })
                .catch(() => alert('修改失败'));
        },
        updateEmail: function (email) {
            if (this.$refs.form.validate()) {
                Post('/Account/UpdateEmail', { value: email })
                    .then(res => res.json())
                    .then(data => {
                        if (!data.isSucceeded)
                            alert(data.errorMessage);
                        else this.getUserInfo();
                    })
                    .catch(() => alert('修改失败'));
            }
        },
        updatePhoneNumber: function (phoneNumber) {
            Post('/Account/UpdatePhoneNumber', { value: phoneNumber })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded)
                        alert(data.errorMessage);
                    else this.getUserInfo();
                })
                .catch(() => alert('修改失败'));
        },
        selectFile: function () {
            document.getElementById('avatar_file').click();
        },
        validateFile: function () {
            let ele = document.getElementById('avatar_file');
            let file = ele.files[0];
            if (!file.type.startsWith('image/')) {
                alert('所选文件不是图片文件');
                ele.value = '';
                return;
            }
            if (file.size > 1048576) {
                alert('图片大小不能超过 1 Mb');
                ele.value = '';
                return;
            }
            let form = new FormData();
            form.append('file', file);
            let token = ReadCookie('XSRF-TOKEN');
            fetch('/Account/UpdateAvatar', {
                method: 'POST',
                credentials: 'same-origin',
                body: form,
                mode: 'cors',
                cache: 'default',
                headers: { 'X-XSRF-TOKEN': token }
            })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) this.getUserInfo();
                    else alert(data.errorMessage);
                    ele.value = '';
                })
                .catch(() => {
                    alert('修改失败');
                    ele.value = '';
                });
        },
        confirmPhoneNumber: function () {
            alert('此功能正在开发中, 敬请期待');
        },
        confirmEmail: function () {
            this.submitting = true;
            Post('/Account/SendEmailConfirmToken', {})
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) this.confirmEmailDialog = true;
                    else alert(data.errorMessage);
                    this.submitting = false;
                })
                .catch(() => {
                    alert('请求失败');
                    this.submitting = false;
                });
        },
        closeDlg: function () {
            this.confirmEmailDialog = false;
        }
    },
    components: {
        emailConfirm: emailConfirm,
        phoneConfirm: phoneConfirm
    }
};