import { Get, Post } from '../../utilities/requestHelper';
import * as signalR from '@aspnet/signalr';

export default {
    // loadParameters : { count: int, others... }
    props: ['loadUrl', 'loadParameters', 'sendUrl', 'sendParameters', 'showSnack'],
    data: () => ({
        chats: [],
        chatLastload: 2147483647,
        inputText: '',
        connection: null,
        msgLoading: true,
        inputRules: [
            v => !!v || '请输入发送内容'
        ],
        currentReply: 0,
        hubConnected: false,
        sending: false
    }),
    mounted: function () {
        let msgList = document.getElementById('msgList');
        if (msgList !== null) {
            this.loadMessages().then(() => {
                msgList.scrollTop = msgList.scrollHeight - msgList.clientHeight;
                msgList.onscroll = () => {
                    if (msgList.scrollTop <= 0) {
                        if (this.chatLastload !== -1) {
                            this.loadMessages(this.chatLastload).then(cnt => {
                                msgList.scrollTop += 157.7 * cnt;
                            });
                        }
                    }
                };
                this.connection = this.buildSingalR();
            });
        }
    },
    destroyed: function () {
        if (this.connection !== null)
            this.connection.stop();
    },
    methods: {
        loadMessages: function (id = 2147483647) {
            let param = this.loadParameters;
            param['startId'] = id;
            return Get(this.loadUrl, param)
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
                    this.showSnack('消息加载失败', 'error', 3000);
                });
        },
        sendMessage: function () {
            if (this.inputText && this.inputText.length <= 65536)
                this.sending = true;
            let param = this.sendParameters;
            param['content'] = this.inputText;
            param['replyId'] = this.currentReply;
            Post(this.sendUrl, param)
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded) {
                        this.showSnack(data.errorMessage, 'error', 3000);
                    } else {
                        this.showSnack('发送成功，获得 5 经验', 'success', 3000);
                        this.currentReply = 0;
                    }
                    this.sending = false;
                })
                .catch(() => {
                    this.showSnack('发送失败', 'error', 3000);
                    this.sending = false;
                });
        },
        cancelReply: function () {
            this.currentReply = 0;
        },
        processReply: function (id) {
            this.currentReply = id;
        },
        buildSingalR: function () {
            let conn = new signalR.HubConnectionBuilder().withUrl('/ChatHub').build();
            conn.on('ChatMessage', (id, userId, userName, sendTime, content, replyId) => {
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
            conn.onclose(error => {
                this.hubConnected = false;
                if (error) {
                    this.connection = this.buildSingalR();
                }
            });
            conn.start()
                .then(() => this.hubConnected = true)
                .catch(() => {
                    this.hubConnected = false;
                    this.connection = this.buildSingalR();
                });
            return conn;
        }
    }
};