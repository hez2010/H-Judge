import { Get } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';
import { ensureLoading } from '../../utilities/scriptHelper';
import { initializeObjects } from '../../utilities/initHelper';

export default {
    props: ['showSnack', 'setMessageCount'],
    data: () => ({
        loading: true
    }),
    mounted: function () {
        setTitle('消息详情');

        initializeObjects({
            message: {},
            timer: null,
            param: {
                msgId: 0
            }
        }, this);

        if (this.$route.params.mid) {
            this.param.msgId = parseInt(this.$route.params.mid);
        }
        
        Get('/Message/GetMessageContent', this.param)
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.message = data;
                    this.$nextTick(() => {
                        ensureLoading('mathjax', 'https://cdn.hjudge.com/hjudge/lib/MathJax-2.7.5/MathJax.js?config=TeX-MML-AM_CHTML', this.loadMath);
                    });
                    if (data.status === 1) this.setMessageCount(2, -1);
                }
                else this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    },
    methods: {
        loadMath: function () {
            this.timer = setInterval(() => {
                if (document.getElementById('details_content')) {
                    clearInterval(this.timer);
                    window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                    this.timer = null;
                }
            }, 1000);
        }
    }
};