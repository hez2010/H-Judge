import { setTitle } from '../../../utilities/titleHelper';
import { Get } from '../../../utilities/requestHelper';

export default {
    data: () => ({
        avatar: '',
        loading: true,
        user: {}
    }),
    mounted: function () {
        setTitle('用户');
        Get('/Account/GetUserInfo', { userId: this.$route.params.uid })
            .then(res => res.json())
            .then(data => {
                if (data.isSignedIn) {
                    this.user = data;
                    if (this.user !== null) {
                        this.privilege = this.user.privilege === 1 ? '管理员' :
                            this.user.privilege === 2 ? '教师' :
                                this.user.privilege === 3 ? '助教' :
                                    this.user.privilege === 4 ? '学生/选手' :
                                        this.user.privilege === 5 ? '黑名单' : '未知';
                    }
                }
                else {
                    alert('该用户不存在');
                }
                this.loading = false;
            })
            .catch(() => {
                alert('加载失败');
                this.loading = false;
            });

        Get('/Account/GetUserAvatar', { userId: this.$route.params.uid })
            .then(res => res.text())
            .then(data => this.avatar = 'data:image/png;base64, ' + data);
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
    }
};