import { setTitle } from '../../utilities/titleHelper';
import * as signalR from '@aspnet/signalr';
import { Get, Post } from '../../utilities/requestHelper';

export default {
    props: ['user', 'updateFortune'],
    data: () => ({
        announcements: [],
        annpage: 0,
        chats: [],
        chatLastload: 2147483647,
        inputText: '',
        connection: null,
        msgLoading: true,
        annLoading: true,
        inputRules: [
            v => !!v || '请输入发送内容'
        ],
        currentReply: 0
    }),
    mounted: function () {
        setTitle('主页');

        this.annLoading = false;

        this.loadMessages().then(() => {
            let msgList = document.getElementById('msgList');

            if (msgList !== null) {
                msgList.scrollTop = msgList.scrollHeight - msgList.clientHeight;

                msgList.onscroll = () => {
                    if (msgList.scrollTop <= 0) {
                        if (this.chatLastload !== -1) {
                            let msgList = document.getElementById('msgList');
                            if (msgList !== null) {
                                this.loadMessages(this.chatLastload).then(cnt => {
                                    msgList.scrollTop += 157.7 * cnt;
                                });
                            }
                        }
                    }
                };
            }

            this.connection = new signalR.HubConnectionBuilder().withUrl('/ChatHub').build();
            this.connection.on('ChatMessage', (id, userId, userName, sendTime, content, replyId) => {
                let data = {
                    id: id,
                    userId: userId,
                    userName: userName,
                    sendTime: sendTime,
                    content: content,
                    replyId: replyId,
                    avatar: '/Account/GetUserAvatar?userId=' + userId
                };
                let msgList = document.getElementById('msgList');
                if (msgList !== null) {
                    let dTop = msgList.scrollHeight - msgList.clientHeight - msgList.scrollTop;
                    this.chats = this.chats.concat([data]);
                    if (dTop <= 100 || !!this.inputText) {
                        this.$nextTick(() => {
                            msgList.scrollTop = msgList.scrollHeight - msgList.clientHeight;
                        });
                    }
                }
                this.inputText = '';
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
                    this.msgLoading = false;
                    return data.length;
                })
                .catch(() => {
                    alert('消息加载失败');
                });
        },
        sendMessage: function () {
            if (this.inputText && this.inputText.length <= 65536)
                Post('/Message/SendChat', { content: this.inputText, replyId: this.currentReply })
                    .then(res => res.json())
                    .then(data => {
                        if (!data.isSucceeded) {
                            alert(data.errorMessage);
                        } else {
                            this.currentReply = 0;
                        }
                    })
                    .catch(() => {
                        alert('发送失败');
                    });
        },
        cancelReply: function () {
            this.currentReply = 0;
        },
        processReply: function (id) {
            this.currentReply = id;
        }
    }
};