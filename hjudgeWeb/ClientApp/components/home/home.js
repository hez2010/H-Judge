import { setTitle } from '../../utilities/titleHelper';
const chatboard = () => import(/* webpackChunkName: 'chatboard' */'../chatboard/chatboard.vue');

export default {
    props: ['user', 'showSnack'],
    data: () => ({
        announcements: [],
        annpage: 0,
        annLoading: true
    }),
    components: {
        chatboard: chatboard
    },
    mounted: function () {
        setTitle('主页');

        this.annLoading = false;
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