<template>
	<v-container>
		<v-card>
			<v-tabs right>
				<v-card-title primary-title>
					<h2>题目详情</h2>
				</v-card-title>
				<v-spacer></v-spacer>
				<v-tab :key="1">
					题目信息
				</v-tab>
				<v-tab :key="2">
					题目描述
				</v-tab>
				<v-tab :key="3">
					提交解答
				</v-tab>
				<v-tab-item :key="1">
					<v-card-text v-if="loading">
						<p>加载中...</p>
					</v-card-text>
					<v-card-text v-else>
						<v-container>
							<v-layout wrap>
								<v-flex xs4>
									<strong>题目名称</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.name}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>创建时间</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.creationTime}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>题目难度</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.level}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>题目类型</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.type}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>出题用户</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.userName}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>当前状态</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.status}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>通过数量</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.acceptCount}}</p>
								</v-flex>
								<v-flex xs4>
									<strong>提交数量</strong>
								</v-flex>
								<v-flex xs8>
									<p>{{problem.submissionCount}}</p>
								</v-flex>
							</v-layout>
						</v-container>
					</v-card-text>
				</v-tab-item>
				<v-tab-item :key="2">
					<v-card-text v-if="loading">
						<p>加载中...</p>
					</v-card-text>
					<v-card-text v-else>
						<v-container v-html="problem.description"></v-container>
					</v-card-text>
				</v-tab-item>
				<v-tab-item :key="3">
					<v-card-text v-if="loading">
						<p>加载中...</p>
					</v-card-text>
					<v-card-text v-else>
						<v-container>
							<v-form ref="form" v-model="valid" lazy-validation>
								<v-textarea outline
											v-model="content"
											:rules="contentRules"
											label="提交内容"
											required>
								</v-textarea>
								<v-btn color="primary" :disabled="!valid || submitting" @click="submit">提交</v-btn>
							</v-form>
						</v-container>
					</v-card-text>
				</v-tab-item>
			</v-tabs>
		</v-card>
	</v-container>
</template>

<script type="text/javascript" src="./problemDetails.js"></script>