import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        loading: true,
        page: 0,
        contests: [],
        pageCount: 0,
        headers: [
            { text: '编号', value: 'id' },
            { text: '名称', value: 'name' },
            { text: '开始时间', value: 'startTime' },
            { text: '结束时间', value: 'endTime' },
            { text: '题目数量', value: 'problemCount' },
            { text: '状态', value: 'status' }
        ]
    }),
    mounted: function () {
        setTitle('比赛');
        if (!this.$route.params.page) {
            this.$router.push('/Contest/1');
            this.page = 1;
        }
        else this.page = parseInt(this.$route.params.page);
        Get('/Contest/GetContestCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
        if (this.user && this.user.privilege >= 1 && this.user.privilege <= 3) {
            this.headers = this.headers.concat([{ text: '操作', value: 'actions', sortable: false }]);
        }
    },
    watch: {
        page: function () {
            this.$router.push('/Contest/' + this.page);
            this.loading = true;
            let param = { start: (this.page - 1) * 10, count: 10 };
            if (this.$route.params.gid) param['gid'] = this.$route.params.gid;
            this.contests = [];
            Get('/Contest/GetContestList', param)
                .then(res => res.json())
                .then(data => {
                    this.contests = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.contests = [];
                    this.loading = false;
                });
        },
        user: function () {
            if (this.user && this.user.privilege >= 1 && this.user.privilege <= 3) {
                this.headers = this.headers.concat([{ text: '操作', value: 'actions', sortable: false }]);
            }
        }
    },
    methods: {
        toDetails: function (id) {
            this.$router.push('/ContestDetails/' + id.toString());
        }
    }
};