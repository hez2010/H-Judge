import { setTitle } from '../../utilities/titleHelper';
import * as signalR from '@aspnet/signalr';
import { Get, Post } from '../../utilities/requestHelper';

export default {
    data: () => ({
        announcements: [],
        annpage: 0,
        chats: [],
        chatLastload: 2147483647,
        inputText: '',
        connection: null
    }),
    mounted: function () {
        setTitle('主页');
        this.loadMessages().then(() => {
            let msgList = document.getElementById('msgList');
            if (msgList !== null)
                msgList.scrollTop = msgList.scrollHeight;
            msgList.onscroll = () => {
                if (msgList.scrollTop <= 0) {
                    if (this.chatLastload !== -1) {
                        this.loadMessages(this.chatLastload);
                    }
                }
            };
            this.connection = new signalR.HubConnectionBuilder().withUrl('/ChatHub').build();
            this.connection.on('ChatMessage', (userId, userName, sendTime, content) => {
                let data = {
                    userId: userId,
                    userName: userName,
                    sendTime: sendTime,
                    content: content,
                    avatar: '/Account/GetUserAvatar?userId=' + userId
                };
                this.chats = this.chats.concat([data]);
                if (this.inputText) {
                    this.$nextTick(() => {
                        let msgList = document.getElementById('msgList');
                        if (msgList !== null)
                            msgList.scrollTop = msgList.scrollHeight;
                    });
                    this.inputText = '';
                }
            });
            this.connection.start();
        });
    },
    destroyed: function () {
        if (this.connection !== null)
            this.connection.stop();
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
        loadMessages: function (id = 2147483647) {
            return Get('/Message/GetChats', { startId: id, count: 10 })
                .then(res => res.json())
                .then(data => {
                    for (var i in data) {
                        data[i]['avatar'] = '/Account/GetUserAvatar?userId=' + data[i]['userId'];
                    }
                    this.chats = data.concat(this.chats);
                    if (data.length > 0) {
                        this.chatLastload = data[0].id;
                    }
                    else {
                        this.chatLastload = -1;
                    }
                })
                .catch(() => {
                    //ignore
                });
        },
        sendMessage: function () {
            Post('/Message/SendChat', { content: this.inputText })
                .then(res => res.json())
                .then(data => {
                    if (data) {
                        //ignore
                    }
                    else {
                        //ignore
                    }
                })
                .catch(() => {
                    //ignore
                });
        }
    }
};