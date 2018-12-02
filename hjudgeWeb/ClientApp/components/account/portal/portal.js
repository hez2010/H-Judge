import { setTitle } from '../../../utilities/titleHelper';
import { Post, ReadCookie, Get } from '../../../utilities/requestHelper';
import { initializeObjects } from '../../../utilities/initHelper';
const emailConfirm = () => import(/* webpackChunkName: 'emailConfirm' */'../verification/email/emailConfirm.vue');
const phoneConfirm = () => import(/* webpackChunkName: 'phoneConfirm' */'../verification/phone/phoneConfirm.vue');

export default {
    props: ['user', 'getUserInfo', 'updateFortune', 'showSnack'],
    data: () => ({
        bottomNav: '1',
        privilege: '',
        emailRules: [
            v => !!v || '请输入邮箱地址',
            v => /.+@.+\..+/.test(v) || '邮箱地址格式不正确'
        ],
        confirmEmailDialog: false,
        submitting: false,
        loading: true,
        cnt: 0
    }),
    mounted: function () {
        setTitle('门户');

        initializeObjects({
            problemSet: []
        }, this);

        if (this.user !== null) {
            this.updateFortune();
            this.privilege = this.user.privilege === 1 ? '管理员' :
                this.user.privilege === 2 ? '教师' :
                    this.user.privilege === 3 ? '助教' :
                        this.user.privilege === 4 ? '学生/选手' :
                            this.user.privilege === 5 ? '黑名单' : '未知';
        }
    },
    watch: {
        user: function () {
            if (this.user !== null) {
                this.updateFortune();
                this.privilege = this.user.privilege === 1 ? '管理员' :
                    this.user.privilege === 2 ? '教师' :
                        this.user.privilege === 3 ? '助教' :
                            this.user.privilege === 4 ? '学生/选手' :
                                this.user.privilege === 5 ? '黑名单' : '未知';
            }
        },
        confirmEmailDialog: function () {
            this.$refs.confirmEmailDlg.clearForm();
        },
        bottomNav: function () {
            if (this.bottomNav === '3') {
                this.loading = true;
                Get('/Status/GetSolvedProblemList?userId=' + this.user.id)
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.problemSet = data.problemSet;
                        } else {
                            this.showSnack(data.errorMessage, 'error', 3000);
                        }
                        this.loading = false;
                    })
                    .catch(() => {
                        this.showSnack('加载失败', 'error', 3000);
                        this.loading = false;
                    });
            }
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
                    if (!data.isSucceeded) {
                        this.showSnack(data.errorMessage, 'error', 3000);
                        this.getUserInfo();
                    }
                    else this.showSnack('修改成功', 'success', 3000);
                })
                .catch(() => {
                    this.showSnack('修改失败', 'error', 3000);
                    this.getUserInfo();
                });
        },
        updateName: function (name) {
            Post('/Account/UpdateName', { value: name })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded) {
                        this.showSnack(data.errorMessage, 'error', 3000);
                        this.getUserInfo();
                    }
                    else this.showSnack('修改成功', 'success', 3000);
                })
                .catch(() => {
                    this.showSnack('修改失败', 'error', 3000);
                    this.getUserInfo();
                });
        },
        updateEmail: function (email) {
            if (this.$refs.form.validate()) {
                Post('/Account/UpdateEmail', { value: email })
                    .then(res => res.json())
                    .then(data => {
                        if (!data.isSucceeded)
                            this.showSnack(data.errorMessage, 'error', 3000);
                        else {
                            this.showSnack('修改成功', 'success', 3000);
                        }
                        this.getUserInfo();
                    })
                    .catch(() => {
                        this.showSnack('修改失败', 'error', 3000);
                        this.getUserInfo();
                    });
            }
        },
        updatePhoneNumber: function (phoneNumber) {
            Post('/Account/UpdatePhoneNumber', { value: phoneNumber })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded)
                        this.showSnack(data.errorMessage, 'error', 3000);
                    else {
                        this.showSnack('修改成功', 'success', 3000);
                    }
                    this.getUserInfo();
                })
                .catch(() => {
                    this.showSnack('修改失败', 'error', 3000);
                    this.getUserInfo();
                });
        },
        selectFile: function () {
            document.getElementById('avatar_file').click();
        },
        validateFile: function () {
            let ele = document.getElementById('avatar_file');
            let file = ele.files[0];
            if (!file.type.startsWith('image/')) {
                this.showSnack('所选文件不是图片文件', 'error', 3000);
                ele.value = '';
                return;
            }
            if (file.size > 1048576) {
                this.showSnack('图片大小不能超过 1 Mb', 'error', 3000);
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
                    else this.showSnack(data.errorMessage, 'error', 3000);
                    ele.value = '';
                    this.cnt++;
                    this.showSnack('修改成功', 'success', 3000);
                })
                .catch(() => {
                    this.showSnack('修改失败', 'error', 3000);
                    ele.value = '';
                });
        },
        confirmPhoneNumber: function () {
            this.showSnack('此功能正在开发中，敬请期待...', 'info', 3000);
        },
        confirmEmail: function () {
            this.submitting = true;
            Post('/Account/SendEmailConfirmToken', {})
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) this.confirmEmailDialog = true;
                    else
                        this.showSnack(data.errorMessage, 'error', 3000);
                    this.submitting = false;
                })
                .catch(() => {
                    this.showSnack('请求失败', 'error', 3000);
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