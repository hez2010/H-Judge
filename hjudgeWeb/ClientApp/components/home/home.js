import { setTitle } from '../../utilities/titleHelper';

export default {
    data: () => ({
        announcements: [
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
            { title: 'test', content: '<h3>hhh</h3>' },
        ],
        items: [
            { title: 'Jason Oner', content: 'asduadnuq82uceadnuq82uceadnuq82uceadnuq82uceadnuq82uceadnuq82uceadnuq82uceadnuq82uceadnuq82uce92', avatar: 'https://cdn.vuetifyjs.com/images/lists/1.jpg', isMe: false },
            { title: 'Travis Howard', content: '#include&lt;iostream&gt;<br />djusdhuds', avatar: 'https://cdn.vuetifyjs.com/images/lists/2.jpg', isMe: true },
            { title: 'Ali Connors', content: 'asduadnuq82uce92', avatar: 'https://cdn.vuetifyjs.com/images/lists/3.jpg', isMe: true },
            { title: 'Cindy Baker', content: 'asduadnuq82uce92', avatar: 'https://cdn.vuetifyjs.com/images/lists/4.jpg', isMe: false }
        ],
        inputText: ''
    }),
    mounted: function () {
        setTitle('主页');
        let msgList = document.getElementById('msgList');
        if (msgList !== null)
            msgList.scrollTop = msgList.scrollHeight;
        msgList.onscroll = () => {
            if (msgList.scrollTop <= 0) {
                //TODO: load msg list
            }
        };
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

    }
};