import { setTitle } from '../../utilities/titleHelper';
import { Get, Post } from '../../utilities/requestHelper';
import { initializeObjects } from '../../utilities/initHelper';

export default {
    props: ['setMessageCount', 'updateMessageCount', 'showSnack', 'user'],
    mounted: function () {
        setTitle('消息');

        if (!this.$route.params.page) this.$router.push('/Message/1');
        else this.page = parseInt(this.$route.params.page);

        initializeObjects({
            messages: []
        }, this);

        Get('/Message/GetMessageCount', { type: 3 })
            .then(res => res.text())
            .then(data => {
                if (data) {
                    this.pageCount = Math.ceil(parseInt(data) / 10);
                    if (this.pageCount === 0) this.pageCount = 1;
                    if (this.page > this.pageCount) this.page = this.pageCount;
                    if (this.page <= 0) this.page = 1;
                }
                else this.pageCount = 0;
            })
            .catch(() => this.pageCount = 0);

        this.load();
    },
    data: () => ({
        loading: true,
        page: 0,
        pageCount: 0,
        headers: [
            { text: '编号', value: 'id' },
            { text: '方向', value: 'direction' },
            { text: '类型', value: 'type' },
            { text: '标题', value: 'title' },
            { text: '用户', value: 'userName' },
            { text: '发送时间', value: 'sendTime' },
            { text: '状态', value: 'status' }
        ]
    }),
    watch: {
        $route: function () {
            let page = this.$route.params.page ? parseInt(this.$route.params.page) : 1;
            this.page = page;
            this.load();
        },
        page: function () {
            this.$router.push('/Message/' + this.page);
        }
    },
    methods: {
        load: function () {
            if (this.page === 0) return;
            let param = { start: (this.page - 1) * 10, count: 10 };
            this.loading = true;
            Get('/Message/GetMessages', param)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.messages = data.messages;
                        this.updateMessageCount();
                    }
                    else this.showSnack(data.errorMessage, 'error', 3000);
                    this.loading = false;
                })
                .catch(() => {
                    this.showSnack('加载失败', 'error', 3000);
                    this.loading = false;
                });
        },
        markAllRead: function () {
            Post('/Message/SetMessageStatus', { status: 2, messageId: 0 })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.setMessageCount(1, 0);
                        this.load();
                    }
                    else this.showSnack(data.errorMessage, 'error', 3000);
                })
                .catch(() => this.showSnack('标记失败', 'error', 3000));
        },
        sortRules: function (items, index, isDescending) {
            if (index === 'id') {
                return items.sort((x, y) => {
                    if (x.id < y.id) return isDescending ? -1 : 1;
                    else if (x.id > y.id) return isDescending ? 1 : -1;
                    else return 0;
                });
            } else {
                return items.sort((x, y) => {
                    if (x.id < y.id) return isDescending ? 1 : -1;
                    else if (x.id > y.id) return isDescending ? -1 : 1;
                    else return 0;
                });
            }
        }
    }
};