import navmenu from '../navmenu/navmenu.vue';
import { Get } from '../../utilities/requestHelper';

export default {
    data: () => ({
        userInfo: {}
    }),
    components: {
        navmenu: navmenu
    },
    mounted() {
        Get('/Account/GetUserInfo')
            .then(res => res.json())
            .then(data => this.userInfo = data)
            .catch(() => alert('网络错误'));
    },
    methods: {

    }
};