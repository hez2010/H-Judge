<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>消息列表</h2>
                <v-spacer></v-spacer>
                <v-tooltip v-if="user && user.emailConfirmed" bottom>
                    <v-btn icon slot="activator" to="/Message/NewMessage/0">
                        <v-icon color="primary">add</v-icon>
                    </v-btn>
                    <span>新建消息</span>
                </v-tooltip>
                <v-tooltip v-if="user && user.emailConfirmed" bottom>
                    <v-btn icon slot="activator" @click="markAllRead">
                        <v-icon color="primary">done</v-icon>
                    </v-btn>
                    <span>全部标记已读</span>
                </v-tooltip>
            </v-card-title>
            <v-card-text>
                <v-data-table :headers="headers"
                              :items="loading ? [] : messages"
                              :custom-sort="sortRules"
                              hide-actions>
                    <template slot="items" slot-scope="props">
                        <td>{{ props.item.id }}</td>
                        <td><router-link :to="{ path: '/MessageDetails/' + props.item.id }">{{ props.item.title }}</router-link></td>
                        <td><router-link :to="{ path: '/Account/' + props.item.userId }">{{ props.item.userName }}</router-link></td>
                        <td>{{ props.item.sendTime }}</td>
                        <td>{{ props.item.status == 1 ? '未读' : '已读' }}</td>
                        <td>{{ props.item.direction == 1 ? '发送' : '接收' }}</td>
                    </template>
                    <template slot="no-data">
                        <p v-if="loading">正在加载...</p>
                        <p v-else>没有数据 :(</p>
                    </template>
                </v-data-table>
            </v-card-text>
            <v-card-actions>
                <v-layout justify-center align-center>
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="5"></v-pagination>
                </v-layout>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./message.js"></script>