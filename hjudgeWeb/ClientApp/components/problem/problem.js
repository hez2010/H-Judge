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
            { text: '通过', value: 'acceptCount' },
            { text: '提交', value: 'submissionCount' }
        ]
    }),
    mounted: function () {
        setTitle('题目');
        Get('/Problem/GetProblemCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                this.page = 1;
            })
            .catch(() => this.pageCount = 0);
    },
    watch: {
        page: function () {
            Get('/Problem/GetProblemList', { start: (this.page - 1) * 10, count: 10 })
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
    }
};