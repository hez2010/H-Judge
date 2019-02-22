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
var Home = /** @class */ (function (_super) {
    __extends(Home, _super);
    function Home() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Home.prototype.componentDidMount = function () {
        titleHelper_1.setTitle('主页');
    };
    Home.prototype.render = function () {
        return (React.createElement("div", null,
            React.createElement(semantic_ui_react_1.Header, { as: 'h1' }, "\u4E3B\u9875")));
    };
    return Home;
}(React.Component));
exports.default = Home;
//# sourceMappingURL=home.js.map