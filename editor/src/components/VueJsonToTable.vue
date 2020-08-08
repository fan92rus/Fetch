<template>
  <div class="table-main">
    <div v-for="(row, index) in arrData" :key="index" class>
      <div class="rows">
        <div class="row key p-2 text-capitalize d-inline-block header-line">{{ keyTitle(row) }}</div>
        <div class="m-2 d-flex">
          <div v-if="!['string', 'number'].includes(checkValueType(data[row]))" class="key p-2 text-capitalize d-inline-block line"></div>
          <div v-if="['string', 'number'].includes(checkValueType(data[row]))">
            <div class="value p-2 d-inline-block">{{ data[row] }}</div>
          </div>
          <div v-else-if="checkValueType(data[row]) === 'array'" class="d-flex wrap">
            <div v-for="(arrRow, index2) in data[row]" :key="index2" class="d-flex wrap-item">
              <div v-if="['string', 'number'].includes(checkValueType(arrRow))">{{ arrRow }}</div>
              <div v-else>
                <VueJsonToTable :data="arrRow" />
              </div>
            </div>
          </div>
          <div v-else>
            <VueJsonToTable :data="data[row]" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import VueJsonToTable from "./VueJsonToTable";

export default {
  name: "VueJsonToTable",
  components: {
    VueJsonToTable,
  },
  props: {
    data: {
      type: Object,
      default: [],
    },
  },
  computed: {
    arrData() {
      return Object.keys(this.data);
    },
  },
  methods: {
    keyTitle(key) {
      return key.split("_").join(" ");
    },
    checkValueType(val) {
      if (typeof val !== "object") {
        return typeof val;
      }
      return Array.isArray(val) ? "array" : "object";
    },
  },
};
</script>

<style lang="scss" scoped>
.m-2 {
  margin: 0.5rem !important;
}

.mx-2 {
  margin-right: 0.5rem !important;
}
.l-b {
  margin: 10px;
  border-left: 2px solid gray;
}
.wrap {
  flex-wrap: wrap;
  justify-content: space-around;
  .wrap-item {
    display: flex;
    flex-grow: 1;
    justify-content: space-between;
    border: 1px solid gray;
    margin: 4px;
    padding: 3px;
    &:hover {
      transition-duration: 1s;
      border: rgb(0, 162, 236) 2px solid;
    }
  }
}
.p-2 {
  max-width: 50em;
  overflow-wrap: break-word;
  padding: 0.2rem !important;
  &.line {
    &:hover {
      transition-delay: 0.2s;
      transition-duration: 0.1s;
      cursor: pointer;
      background: rgb(0, 162, 236);
    }
    padding: 1rem !important;
  }
}
.header-line {
  margin-top: 0.5em;
  max-width: 300px;
  width: 100%;
}
.d-flex {
  display: flex !important;
  flex-direction: row;
}

.d-inline-block {
  display: inline-block !important;
}

.key {
  background: lightgray;
}

.table-main {
  .row-data {
    border: 1.2px solid grey;
    border-radius: 2px;
  }
}
</style>
