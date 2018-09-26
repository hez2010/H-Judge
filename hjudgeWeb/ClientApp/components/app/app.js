import navmenu from '../navmenu/navmenu.vue';
import { Get } from '../../utilities/requestHelper';

export default {
    data: {
        userInfo: null
    },
    components: {
        navmenu: navmenu
    },
    mounted() {
        Get('/Account/GetUserInfo')
            .then(res => res.json())
            .then(data => this.data.userInfo = data)
            .catch(() => alert('网络错误'));
    },
    methods: {
        test: function () {
            alert('hhss');
        }
    }
};