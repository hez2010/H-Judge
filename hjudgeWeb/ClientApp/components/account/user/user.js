import { setTitle } from '../../../utilities/titleHelper';
import { Get } from '../../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        avatar: '',
        loading: true,
        userInfo: {}
    }),
    mounted: function () {
        setTitle('用户');
        Get('/Account/GetUserInfo', { userId: this.$route.params.uid })
            .then(res => res.json())
            .then(data => {
                if (data.isSignedIn) {
                    this.userInfo = data;
                    if (this.userInfo !== null) {
                        this.privilege = this.userInfo.privilege === 1 ? '管理员' :
                            this.userInfo.privilege === 2 ? '教师' :
                                this.userInfo.privilege === 3 ? '助教' :
                                    this.userInfo.privilege === 4 ? '学生/选手' :
                                        this.userInfo.privilege === 5 ? '黑名单' : '未知';
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