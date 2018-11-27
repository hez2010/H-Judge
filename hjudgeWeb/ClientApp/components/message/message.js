import { setTitle } from '../../utilities/titleHelper';
import { Get, Post } from '../../utilities/requestHelper';
import { initializeObjects } from '../../../utilities/initHelper';

export default {
    props: ['setMessageCount', 'updateMessageCount', 'showSnack'],
    mounted: function () {
        setTitle('消息');

        if (!this.$route.params.page) this.$router.push('/Message/1');
        initializeObjects({
            messages: []
        }, this);

        Get('/Message/GetMessageCount', { type: 3 })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.pageCount = Math.ceil(data.count / 10);
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
        pageCount: 0
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
            let param = { start: (this.page - 1) * 10, count: 10 };
            Get('/Message/GetMessages', param)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.messages = data.messages;
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
            Post('/Message/SetMessageStatus', { status: 2, message: 0 })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.setMessageCount(1, 0);
                    }
                    else this.showSnack(data.errorMessage, 'error', 3000);
                })
                .catch(() => this.showSnack('标记失败', 'error', 3000));
        }
    }
};