<template>
    <v-container grid-list-md text-xs-center>
        <v-layout row wrap>
            <v-flex xs12>
                <v-card>
                    <v-card-title primary-title>
                        <h2>个人账户</h2>
                    </v-card-title>
                    <v-card-text>
                        <v-container v-if="user.isSignedIn">
                            <v-layout wrap v-bind="isColumn">
                                <v-flex xs3>
                                    <v-avatar color="grey lighten-4" size="80">
                                        <img :src="`${'/Account/GetUserAvatar/' + user.id}`" />
                                    </v-avatar>
                                    <div>
                                        <v-chip>
                                            <span>{{user.userName}} ({{privilege}})</span>
                                        </v-chip>
                                    </div>
                                    <div>
                                        <a @click="selectFile">更换头像</a>
                                        <input type="file" id="avatar_file" @change="validateFile" accept="image/*" style="filter: alpha(opacity=0); opacity: 0; width: 0; height: 0;" />
                                    </div>
                                    <br />
                                    <div>
                                        <span>金币</span>
                                        <v-chip>
                                            <span>{{user.coins}}</span>
                                        </v-chip>
                                        <span>经验</span>
                                        <v-chip>
                                            <span>{{user.experience}}</span>
                                        </v-chip>
                                    </div>
                                </v-flex>
                                <v-flex xs9>
                                    <v-card flat>
                                        <div v-if="bottomNav === '1'">
                                            <v-layout wrap v-bind="isColumn">
                                                <v-flex xs7>
                                                    <v-form ref="form" lazy-validation>
                                                        <v-text-field outline
                                                                      v-model="user.name"
                                                                      label="姓名"
                                                                      placeholder="请输入"
                                                                      @change="updateName">
                                                        </v-text-field>
                                                        <v-text-field outline
                                                                      v-if="user.emailConfirmed"
                                                                      v-model="user.email"
                                                                      label="邮箱地址"
                                                                      :rules="emailRules"
                                                                      placeholder="请输入"
                                                                      @change="updateEmail"
                                                                      required>
                                                        </v-text-field>
                                                        <v-text-field outline
                                                                      v-else
                                                                      v-model="user.email"
                                                                      label="邮箱地址 (未验证)"
                                                                      :rules="emailRules"
                                                                      placeholder="请输入"
                                                                      @change="updateEmail"
                                                                      required>
                                                        </v-text-field>
                                                        <v-text-field outline
                                                                      v-if="user.phoneNumberConfirmed"
                                                                      v-model="user.phoneNumber"
                                                                      label="手机号码"
                                                                      placeholder="请输入"
                                                                      @change="updatePhoneNumber">
                                                        </v-text-field>
                                                        <v-text-field outline
                                                                      v-else
                                                                      v-model="user.phoneNumber"
                                                                      label="手机号码 (未验证)"
                                                                      placeholder="请输入"
                                                                      @change="updatePhoneNumber">
                                                        </v-text-field>
                                                    </v-form>
                                                </v-flex>
                                                <v-flex xs5>
                                                    <v-layout wrap v-bind="isColumnR">
                                                        <v-flex xs6>
                                                            <p>
                                                                <v-dialog v-model="confirmEmailDialog" width="500" :disabled="user.emailConfirmed">
                                                                    <v-btn slot="activator" color="primary" :disabled="submitting || user.emailConfirmed" @click="confirmEmail">验证邮箱地址</v-btn>
                                                                    <emailConfirm ref="confirmEmailDlg" :getUserInfo="getUserInfo" :closeDlg="closeDlg"></emailConfirm>
                                                                </v-dialog>
                                                            </p>
                                                        </v-flex>
                                                        <v-flex xs6>
                                                            <p>
                                                                <v-btn color="primary" :disabled="submitting || user.phoneNumberConfirmed" @click="confirmPhoneNumber">验证手机号码</v-btn>
                                                            </p>
                                                        </v-flex>
                                                    </v-layout>
                                                </v-flex>
                                            </v-layout>
                                        </div>
                                        <div v-else-if="bottomNav === '2'">
                                            <template v-for="item in user.otherInfo">
                                                <v-text-field outline
                                                              :key="item.key"
                                                              v-model="item.value"
                                                              :label="item.name"
                                                              placeholder="请输入"
                                                              @change="updateOtherInfo">
                                                </v-text-field>
                                            </template>
                                        </div>
                                        <br />
                                        <br />
                                        <br />
                                        <v-bottom-nav :active.sync="bottomNav"
                                                      :value="true"
                                                      absolute
                                                      color="transparent">
                                            <v-btn color="teal"
                                                   flat
                                                   value="1">
                                                <span>基本信息</span>
                                                <v-icon>info</v-icon>
                                            </v-btn>
                                            <v-btn color="teal"
                                                   flat
                                                   value="2">
                                                <span>其他信息</span>
                                                <v-icon>apps</v-icon>
                                            </v-btn>
                                        </v-bottom-nav>
                                    </v-card>
                                </v-flex>
                            </v-layout>
                        </v-container>
                        <v-container v-else>
                            <p>请先登录</p>
                        </v-container>
                    </v-card-text>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script type="text/javascript" src="./portal.js"></script>