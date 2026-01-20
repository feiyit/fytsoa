<template>
  <el-input v-model="defaultValue" v-bind="$attrs">
    <template #append>
      <el-dropdown size="medium" @command="handleShortcuts">
        <el-button type="primary">
          <el-icon class="el-icon--right"><arrow-down /></el-icon>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="0 * * * * ?">每分钟</el-dropdown-item>
            <el-dropdown-item command="0 0 * * * ?">每小时</el-dropdown-item>
            <el-dropdown-item command="0 0 0 * * ?">每天零点</el-dropdown-item>
            <el-dropdown-item command="0 0 0 1 * ?"
              >每月一号零点</el-dropdown-item
            >
            <el-dropdown-item command="0 0 0 L * ?"
              >每月最后一天零点</el-dropdown-item
            >
            <el-dropdown-item command="0 0 0 ? * 1"
              >每周星期日零点</el-dropdown-item
            >
            <el-dropdown-item
              v-for="(item, index) in shortcuts"
              :key="item.value"
              :divided="index === 0"
              :command="item.value"
              >{{ item.text }}</el-dropdown-item
            >
            <el-dropdown-item divided command="custom">自定义</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </template>
  </el-input>

  <el-dialog
    title="cron规则生成器"
    v-model="dialogVisible"
    :width="580"
    destroy-on-close
    append-to-body
  >
    <div class="sc-cron">
      <el-tabs>
        <!-- 秒 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>秒</h2>
              <h4>{{ valueSecond }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.second.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.second.type === '1'">
              <el-input-number
                v-model="value.second.range.start"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.second.range.end"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.second.type === '2'">
              <el-input-number
                v-model="value.second.loop.start"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              秒开始，每
              <el-input-number
                v-model="value.second.loop.end"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              秒执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.second.type === '3'">
              <el-select
                v-model="value.second.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.second"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 分钟 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>分钟</h2>
              <h4>{{ valueMinute }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.minute.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.minute.type === '1'">
              <el-input-number
                v-model="value.minute.range.start"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.minute.range.end"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.minute.type === '2'">
              <el-input-number
                v-model="value.minute.loop.start"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              分钟开始，每
              <el-input-number
                v-model="value.minute.loop.end"
                :min="0"
                :max="59"
                controls-position="right"
              ></el-input-number>
              分钟执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.minute.type === '3'">
              <el-select
                v-model="value.minute.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.minute"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 小时 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>小时</h2>
              <h4>{{ valueHour }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.hour.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.hour.type === '1'">
              <el-input-number
                v-model="value.hour.range.start"
                :min="0"
                :max="23"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.hour.range.end"
                :min="0"
                :max="23"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.hour.type === '2'">
              <el-input-number
                v-model="value.hour.loop.start"
                :min="0"
                :max="23"
                controls-position="right"
              ></el-input-number>
              小时开始，每
              <el-input-number
                v-model="value.hour.loop.end"
                :min="0"
                :max="23"
                controls-position="right"
              ></el-input-number>
              小时执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.hour.type === '3'">
              <el-select
                v-model="value.hour.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.hour"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 日 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>日</h2>
              <h4>{{ valueDay }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.day.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
                <el-radio-button label="4">本月最后一天</el-radio-button>
                <el-radio-button label="5">不指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.day.type === '1'">
              <el-input-number
                v-model="value.day.range.start"
                :min="1"
                :max="31"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.day.range.end"
                :min="1"
                :max="31"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.day.type === '2'">
              <el-input-number
                v-model="value.day.loop.start"
                :min="1"
                :max="31"
                controls-position="right"
              ></el-input-number>
              号开始，每
              <el-input-number
                v-model="value.day.loop.end"
                :min="1"
                :max="31"
                controls-position="right"
              ></el-input-number>
              天执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.day.type === '3'">
              <el-select
                v-model="value.day.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.day"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 月 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>月</h2>
              <h4>{{ valueMonth }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.month.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.month.type === '1'">
              <el-input-number
                v-model="value.month.range.start"
                :min="1"
                :max="12"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.month.range.end"
                :min="1"
                :max="12"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.month.type === '2'">
              <el-input-number
                v-model="value.month.loop.start"
                :min="1"
                :max="12"
                controls-position="right"
              ></el-input-number>
              月开始，每
              <el-input-number
                v-model="value.month.loop.end"
                :min="1"
                :max="12"
                controls-position="right"
              ></el-input-number>
              月执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.month.type === '3'">
              <el-select
                v-model="value.month.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.month"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 周 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>周</h2>
              <h4>{{ valueWeek }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.week.type">
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
                <el-radio-button label="4">本月最后一周</el-radio-button>
                <el-radio-button label="5">不指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.week.type === '1'">
              <el-select v-model="value.week.range.start">
                <el-option
                  v-for="(item, index) in data.week"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
              <span style="padding: 0 15px">-</span>
              <el-select v-model="value.week.range.end">
                <el-option
                  v-for="(item, index) in data.week"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.week.type === '2'">
              第
              <el-input-number
                v-model="value.week.loop.start"
                :min="1"
                :max="4"
                controls-position="right"
              ></el-input-number>
              周的星期
              <el-select v-model="value.week.loop.end">
                <el-option
                  v-for="(item, index) in data.week"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
              执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.week.type === '3'">
              <el-select
                v-model="value.week.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.week"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="最后一周" v-if="value.week.type === '4'">
              <el-select v-model="value.week.last">
                <el-option
                  v-for="(item, index) in data.week"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 年 -->
        <el-tab-pane>
          <template #label>
            <div class="sc-cron-num">
              <h2>年</h2>
              <h4>{{ valueYear }}</h4>
            </div>
          </template>
          <el-form>
            <el-form-item label="类型">
              <el-radio-group v-model="value.year.type">
                <el-radio-button label="-1">忽略</el-radio-button>
                <el-radio-button label="0">任意值</el-radio-button>
                <el-radio-button label="1">范围</el-radio-button>
                <el-radio-button label="2">间隔</el-radio-button>
                <el-radio-button label="3">指定</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="范围" v-if="value.year.type === '1'">
              <el-input-number
                v-model="value.year.range.start"
                controls-position="right"
              ></el-input-number>
              <span style="padding: 0 15px">-</span>
              <el-input-number
                v-model="value.year.range.end"
                controls-position="right"
              ></el-input-number>
            </el-form-item>
            <el-form-item label="间隔" v-if="value.year.type === '2'">
              <el-input-number
                v-model="value.year.loop.start"
                controls-position="right"
              ></el-input-number>
              年开始，每
              <el-input-number
                v-model="value.year.loop.end"
                :min="1"
                controls-position="right"
              ></el-input-number>
              年执行一次
            </el-form-item>
            <el-form-item label="指定" v-if="value.year.type === '3'">
              <el-select
                v-model="value.year.appoint"
                multiple
                style="width: 100%"
              >
                <el-option
                  v-for="(item, index) in data.year"
                  :key="index"
                  :label="item"
                  :value="item"
                ></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </div>

    <template #footer>
      <el-button @click="dialogVisible = false">取 消</el-button>
      <el-button type="primary" @click="submit">确 认</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed, watch, onMounted, getCurrentInstance } from 'vue';

// 获取组件实例用于 emit
const instance = getCurrentInstance();
const emit = instance.emit;

// 定义 props
const props = defineProps({
  modelValue: {
    type: String,
    default: '* * * * * ?',
  },
  shortcuts: {
    type: Array,
    default: () => [],
  },
});

// 响应式变量
const defaultValue = ref('');
const dialogVisible = ref(false);

// 初始化值结构
const value = ref({
  second: {
    type: '0',
    range: { start: 1, end: 2 },
    loop: { start: 0, end: 1 },
    appoint: [],
  },
  minute: {
    type: '0',
    range: { start: 1, end: 2 },
    loop: { start: 0, end: 1 },
    appoint: [],
  },
  hour: {
    type: '0',
    range: { start: 1, end: 2 },
    loop: { start: 0, end: 1 },
    appoint: [],
  },
  day: {
    type: '0',
    range: { start: 1, end: 2 },
    loop: { start: 1, end: 1 },
    appoint: [],
  },
  month: {
    type: '0',
    range: { start: 1, end: 2 },
    loop: { start: 1, end: 1 },
    appoint: [],
  },
  week: {
    type: '5',
    range: { start: '2', end: '3' },
    loop: { start: 0, end: '2' },
    last: '2',
    appoint: [],
  },
  year: {
    type: '-1',
    range: { start: getYear()[0], end: getYear()[1] },
    loop: { start: getYear()[0], end: 1 },
    appoint: [],
  },
});

// 数据选项
const data = ref({
  second: [
    '0',
    '5',
    '15',
    '20',
    '25',
    '30',
    '35',
    '40',
    '45',
    '50',
    '55',
    '59',
  ],
  minute: [
    '0',
    '5',
    '15',
    '20',
    '25',
    '30',
    '35',
    '40',
    '45',
    '50',
    '55',
    '59',
  ],
  hour: [
    '0',
    '1',
    '2',
    '3',
    '4',
    '5',
    '6',
    '7',
    '8',
    '9',
    '10',
    '11',
    '12',
    '13',
    '14',
    '15',
    '16',
    '17',
    '18',
    '19',
    '20',
    '21',
    '22',
    '23',
  ],
  day: [
    '1',
    '2',
    '3',
    '4',
    '5',
    '6',
    '7',
    '8',
    '9',
    '10',
    '11',
    '12',
    '13',
    '14',
    '15',
    '16',
    '17',
    '18',
    '19',
    '20',
    '21',
    '22',
    '23',
    '24',
    '25',
    '26',
    '27',
    '28',
    '29',
    '30',
    '31',
  ],
  month: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'],
  week: [
    { value: '1', label: '周日' },
    { value: '2', label: '周一' },
    { value: '3', label: '周二' },
    { value: '4', label: '周三' },
    { value: '5', label: '周四' },
    { value: '6', label: '周五' },
    { value: '7', label: '周六' },
  ],
  year: getYear(),
});

// 计算属性 - 各时间单位的 cron 表达式
const valueSecond = computed(() => {
  const v = value.value.second;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  return '*';
});

const valueMinute = computed(() => {
  const v = value.value.minute;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  return '*';
});

const valueHour = computed(() => {
  const v = value.value.hour;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  return '*';
});

const valueDay = computed(() => {
  const v = value.value.day;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  if (v.type === '4') return 'L';
  if (v.type === '5') return '?';
  return '*';
});

const valueMonth = computed(() => {
  const v = value.value.month;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  return '*';
});

const valueWeek = computed(() => {
  const v = value.value.week;
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.end}#${v.loop.start}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '*';
  if (v.type === '4') return `${v.last}L`;
  if (v.type === '5') return '?';
  return '*';
});

const valueYear = computed(() => {
  const v = value.value.year;
  if (v.type === '-1') return '';
  if (v.type === '0') return '*';
  if (v.type === '1') return `${v.range.start}-${v.range.end}`;
  if (v.type === '2') return `${v.loop.start}/${v.loop.end}`;
  if (v.type === '3') return v.appoint.length > 0 ? v.appoint.join(',') : '';
  return '';
});

// 监听 week.type 和 day.type 的互斥关系
watch(
  () => value.value.week.type,
  (val) => {
    if (val !== '5') {
      value.value.day.type = '5';
    }
  },
);

watch(
  () => value.value.day.type,
  (val) => {
    if (val !== '5') {
      value.value.week.type = '5';
    }
  },
);

// 生命周期 - 组件挂载时初始化
onMounted(() => {
  defaultValue.value = props.modelValue;
});

// 方法
function handleShortcuts(command) {
  if (command === 'custom') {
    open();
  } else {
    defaultValue.value = command;
    emit('update:modelValue', defaultValue.value);
  }
}

function open() {
  set();
  dialogVisible.value = true;
}

function set() {
  defaultValue.value = props.modelValue;
  let arr = (defaultValue.value || '* * * * * ?').split(' ');

  // 简单的表达式校验
  if (arr.length < 6) {
    instance.appContext.config.globalProperties.$message.warning(
      'cron表达式错误，已转换为默认表达式',
    );
    arr = '* * * * * ?'.split(' ');
  }

  // 解析秒
  if (arr[0] === '*') {
    value.value.second.type = '0';
  } else if (arr[0].includes('-')) {
    value.value.second.type = '1';
    value.value.second.range.start = Number(arr[0].split('-')[0]);
    value.value.second.range.end = Number(arr[0].split('-')[1]);
  } else if (arr[0].includes('/')) {
    value.value.second.type = '2';
    value.value.second.loop.start = Number(arr[0].split('/')[0]);
    value.value.second.loop.end = Number(arr[0].split('/')[1]);
  } else {
    value.value.second.type = '3';
    value.value.second.appoint = arr[0].split(',');
  }

  // 解析分钟
  if (arr[1] === '*') {
    value.value.minute.type = '0';
  } else if (arr[1].includes('-')) {
    value.value.minute.type = '1';
    value.value.minute.range.start = Number(arr[1].split('-')[0]);
    value.value.minute.range.end = Number(arr[1].split('-')[1]);
  } else if (arr[1].includes('/')) {
    value.value.minute.type = '2';
    value.value.minute.loop.start = Number(arr[1].split('/')[0]);
    value.value.minute.loop.end = Number(arr[1].split('/')[1]);
  } else {
    value.value.minute.type = '3';
    value.value.minute.appoint = arr[1].split(',');
  }

  // 解析小时
  if (arr[2] === '*') {
    value.value.hour.type = '0';
  } else if (arr[2].includes('-')) {
    value.value.hour.type = '1';
    value.value.hour.range.start = Number(arr[2].split('-')[0]);
    value.value.hour.range.end = Number(arr[2].split('-')[1]);
  } else if (arr[2].includes('/')) {
    value.value.hour.type = '2';
    value.value.hour.loop.start = Number(arr[2].split('/')[0]);
    value.value.hour.loop.end = Number(arr[2].split('/')[1]);
  } else {
    value.value.hour.type = '3';
    value.value.hour.appoint = arr[2].split(',');
  }

  // 解析日
  if (arr[3] === '*') {
    value.value.day.type = '0';
  } else if (arr[3] === 'L') {
    value.value.day.type = '4';
  } else if (arr[3] === '?') {
    value.value.day.type = '5';
  } else if (arr[3].includes('-')) {
    value.value.day.type = '1';
    value.value.day.range.start = Number(arr[3].split('-')[0]);
    value.value.day.range.end = Number(arr[3].split('-')[1]);
  } else if (arr[3].includes('/')) {
    value.value.day.type = '2';
    value.value.day.loop.start = Number(arr[3].split('/')[0]);
    value.value.day.loop.end = Number(arr[3].split('/')[1]);
  } else {
    value.value.day.type = '3';
    value.value.day.appoint = arr[3].split(',');
  }

  // 解析月
  if (arr[4] === '*') {
    value.value.month.type = '0';
  } else if (arr[4].includes('-')) {
    value.value.month.type = '1';
    value.value.month.range.start = Number(arr[4].split('-')[0]);
    value.value.month.range.end = Number(arr[4].split('-')[1]);
  } else if (arr[4].includes('/')) {
    value.value.month.type = '2';
    value.value.month.loop.start = Number(arr[4].split('/')[0]);
    value.value.month.loop.end = Number(arr[4].split('/')[1]);
  } else {
    value.value.month.type = '3';
    value.value.month.appoint = arr[4].split(',');
  }

  // 解析周
  if (arr[5] === '*') {
    value.value.week.type = '0';
  } else if (arr[5] === '?') {
    value.value.week.type = '5';
  } else if (arr[5].includes('-')) {
    value.value.week.type = '1';
    value.value.week.range.start = arr[5].split('-')[0];
    value.value.week.range.end = arr[5].split('-')[1];
  } else if (arr[5].includes('#')) {
    value.value.week.type = '2';
    value.value.week.loop.start = Number(arr[5].split('#')[1]);
    value.value.week.loop.end = arr[5].split('#')[0];
  } else if (arr[5].includes('L')) {
    value.value.week.type = '4';
    value.value.week.last = arr[5].split('L')[0];
  } else {
    value.value.week.type = '3';
    value.value.week.appoint = arr[5].split(',');
  }

  // 解析年
  if (!arr[6]) {
    value.value.year.type = '-1';
  } else if (arr[6] === '*') {
    value.value.year.type = '0';
  } else if (arr[6].includes('-')) {
    value.value.year.type = '1';
    value.value.year.range.start = Number(arr[6].split('-')[0]);
    value.value.year.range.end = Number(arr[6].split('-')[1]);
  } else if (arr[6].includes('/')) {
    value.value.year.type = '2';
    value.value.year.loop.start = Number(arr[6].split('/')[1]);
    value.value.year.loop.end = Number(arr[6].split('/')[0]);
  } else {
    value.value.year.type = '3';
    value.value.year.appoint = arr[6].split(',');
  }
}

function getYear() {
  const yearList = [];
  const currentYear = new Date().getFullYear();
  for (let i = 0; i < 11; i++) {
    yearList.push(currentYear + i);
  }
  return yearList;
}

function submit() {
  const year = valueYear.value ? ` ${valueYear.value}` : '';
  defaultValue.value = `${valueSecond.value} ${valueMinute.value} ${valueHour.value} ${valueDay.value} ${valueMonth.value} ${valueWeek.value}${year}`;
  emit('update:modelValue', defaultValue.value);
  dialogVisible.value = false;
}
</script>

<style scoped>
.sc-cron:deep(.el-tabs__item) {
  height: auto;
  line-height: 1;
  padding: 0 7px;
  vertical-align: bottom;
}

.sc-cron-num {
  text-align: center;
  margin-bottom: 15px;
  width: 100%;
}

.sc-cron-num h2 {
  font-size: 12px;
  margin-bottom: 15px;
  font-weight: normal;
}

.sc-cron-num h4 {
  display: block;
  height: 32px;
  line-height: 30px;
  width: 100%;
  font-size: 12px;
  padding: 0 15px;
  background: var(--el-color-primary-light-9);
  border-radius: 4px;
}

.sc-cron:deep(.el-tabs__item.is-active) .sc-cron-num h4 {
  background: var(--el-color-primary);
  color: #fff;
}

[data-theme='dark'] .sc-cron-num h4 {
  background: var(--el-color-white);
}
</style>
