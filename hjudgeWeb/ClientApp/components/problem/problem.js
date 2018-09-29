import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';

export default {
    data: () => ({
        loading: true,
        page: 0,
        problems: [],
        pageCount: 0,
        headers: [
            {
                text: '编号',
                align: 'left',
                sortable: true,
                value: 'id'
            },
            { text: '名称', value: 'name' },
            { text: '添加时间', value: 'creationTime' },
            { text: '类型', value: 'type' },
            { text: '难度', value: 'level' },
            { text: '状态', value: 'status' },
            { text: '通过量', value: 'acceptCount' },
            { text: '提交量', value: 'submissionCount' }
        ]
    }),
    mounted: function () {
        setTitle('题目');
        if (!this.$route.params.page) {
            this.$router.push('/Problem/1');
            this.page = 1;
        }
        else this.page = parseInt(this.$route.params.page);
        Get('/Problem/GetProblemCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.page > this.pageCount)
                    this.page = this.pageCount;
            })
            .catch(() => this.pageCount = 0);
    },
    watch: {
        page: function () {
            this.$router.push('/Problem/' + this.page);
            this.loading = true;
            let param = { start: (this.page - 1) * 10, count: 10 };
            if (this.$route.params.cid) param['cid'] = this.$route.params.cid;
            if (this.$route.params.gid) param['gid'] = this.$route.params.gid;
            this.problems = [];
            Get('/Problem/GetProblemList', param)
                .then(res => res.json())
                .then(data => {
                    this.problems = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.problems = [];
                    this.loading = false;
                });
        }
    },
    methods: {
        toDetails: function (id) {
            this.$router.push('/ProblemDetails/' + id.toString());
        }
    }
};