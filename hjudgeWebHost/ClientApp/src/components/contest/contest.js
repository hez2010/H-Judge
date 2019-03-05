"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var titleHelper_1 = require("../../utils/titleHelper");
var semantic_ui_react_1 = require("semantic-ui-react");
var requestHelper_1 = require("../../utils/requestHelper");
var formHelper_1 = require("../../utils/formHelper");
var Contest = /** @class */ (function (_super) {
    __extends(Contest, _super);
    function Contest(props) {
        var _this = _super.call(this, props) || this;
        _this.renderContestList = _this.renderContestList.bind(_this);
        _this.fetchContestList = _this.fetchContestList.bind(_this);
        _this.gotoDetails = _this.gotoDetails.bind(_this);
        _this.state = {
            contestList: {
                contests: [],
                totalCount: 0
            },
            statusFilter: [0, 1, 2]
        };
        return _this;
    }
    Contest.prototype.fetchContestList = function (requireTotalCount, page) {
        var _this = this;
        this.props.history.replace("/contest/" + page);
        var form = document.getElementById('filterForm');
        var req = {};
        req.filter = formHelper_1.SerializeForm(form);
        if (!req.filter.id)
            req.filter.id = 0;
        req.filter.status = this.state.statusFilter;
        req.start = (page - 1) * 10;
        req.count = 10;
        req.requireTotalCount = requireTotalCount;
        req.groupId = this.props.groupId;
        requestHelper_1.Post('/Contest/ContestList', req)
            .then(function (res) { return res.json(); })
            .then(function (data) {
            var result = data;
            if (result.succeeded) {
                var countBackup = _this.state.contestList.totalCount;
                if (!requireTotalCount)
                    result.totalCount = countBackup;
                for (var c in result.contests) {
                    result.contests[c].startTime = new Date(result.contests[c].startTime.toString());
                    result.contests[c].endTime = new Date(result.contests[c].endTime.toString());
                }
                _this.setState({
                    contestList: result
                });
            }
            else {
                _this.props.openPortal('错误', "\u6BD4\u8D5B\u5217\u8868\u52A0\u8F7D\u5931\u8D25\n" + result.errorMessage + " (" + result.errorCode + ")", 'red');
            }
        })
            .catch(function (err) {
            _this.props.openPortal('错误', '比赛列表加载失败', 'red');
            console.log(err);
        });
    };
    Contest.prototype.componentDidMount = function () {
        titleHelper_1.setTitle('比赛');
        if (!this.props.match.params.page) {
            this.fetchContestList(true, 1);
        }
        else
            this.fetchContestList(true, this.props.match.params.page);
    };
    Contest.prototype.gotoDetails = function (index) {
    };
    Contest.prototype.renderContestList = function () {
        var _this = this;
        return React.createElement(React.Fragment, null,
            React.createElement(semantic_ui_react_1.Table, { color: 'blue', selectable: true },
                React.createElement(semantic_ui_react_1.Table.Header, null,
                    React.createElement(semantic_ui_react_1.Table.Row, null,
                        React.createElement(semantic_ui_react_1.Table.HeaderCell, null, "\u7F16\u53F7"),
                        React.createElement(semantic_ui_react_1.Table.HeaderCell, null, "\u540D\u79F0"),
                        React.createElement(semantic_ui_react_1.Table.HeaderCell, null, "\u72B6\u6001"),
                        React.createElement(semantic_ui_react_1.Table.HeaderCell, null, "\u5F00\u59CB\u65F6\u95F4"),
                        React.createElement(semantic_ui_react_1.Table.HeaderCell, null, "\u7ED3\u675F\u65F6\u95F4"))),
                React.createElement(semantic_ui_react_1.Table.Body, null, this.state.contestList.contests.map(function (v, i) {
                    return React.createElement(semantic_ui_react_1.Table.Row, { key: i, warning: v.hidden, onClick: function () { return _this.gotoDetails(i); } },
                        React.createElement(semantic_ui_react_1.Table.Cell, null, v.id),
                        React.createElement(semantic_ui_react_1.Table.Cell, null, v.name),
                        React.createElement(semantic_ui_react_1.Table.Cell, null, v.status === 0 ? '未开始' : v.status === 1 ? '进行中' : '已结束'),
                        React.createElement(semantic_ui_react_1.Table.Cell, null, v.startTime.toLocaleString(undefined, { hour12: false })),
                        React.createElement(semantic_ui_react_1.Table.Cell, null, v.endTime.toLocaleString(undefined, { hour12: false })));
                }))));
    };
    Contest.prototype.render = function () {
        var _this = this;
        return React.createElement(React.Fragment, null,
            React.createElement(semantic_ui_react_1.Form, { id: 'filterForm' },
                React.createElement(semantic_ui_react_1.Form.Group, { widths: 'equal' },
                    React.createElement(semantic_ui_react_1.Form.Field, { width: 6 },
                        React.createElement(semantic_ui_react_1.Label, null, "\u6BD4\u8D5B\u7F16\u53F7"),
                        React.createElement(semantic_ui_react_1.Input, { fluid: true, name: 'id', type: 'number' })),
                    React.createElement(semantic_ui_react_1.Form.Field, null,
                        React.createElement(semantic_ui_react_1.Label, null, "\u6BD4\u8D5B\u540D\u79F0"),
                        React.createElement(semantic_ui_react_1.Input, { fluid: true, name: 'name' })),
                    React.createElement(semantic_ui_react_1.Form.Field, null,
                        React.createElement(semantic_ui_react_1.Label, null, "\u6BD4\u8D5B\u72B6\u6001"),
                        React.createElement(semantic_ui_react_1.Select, { onChange: function (_event, data) { _this.setState({ statusFilter: data.value }); }, fluid: true, name: 'status', multiple: true, defaultValue: [0, 1, 2], options: [{ text: '未开始', value: 0 }, { text: '进行中', value: 1 }, { text: '已结束', value: 2 }] })),
                    React.createElement(semantic_ui_react_1.Form.Field, { width: 4 },
                        React.createElement(semantic_ui_react_1.Label, null, "\u7B5B\u9009\u64CD\u4F5C"),
                        React.createElement(semantic_ui_react_1.Button, { fluid: true, primary: true, onClick: function () { return _this.fetchContestList(true, 1); } }, "\u7B5B\u9009")))),
            this.renderContestList(),
            React.createElement("div", { style: { textAlign: 'center' } },
                React.createElement(semantic_ui_react_1.Pagination, { activePage: this.props.match.params.page, onPageChange: function (_event, data) { return _this.fetchContestList(false, data.activePage); }, size: 'small', siblingRange: 3, boundaryRange: 1, totalPages: Math.floor(this.state.contestList.totalCount / 10) + (this.state.contestList.totalCount % 10 === 0 ? 0 : 1), firstItem: null, lastItem: null })));
    };
    return Contest;
}(React.Component));
exports.default = Contest;
//# sourceMappingURL=contest.js.map